using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using Suitex.Data;
using Suitex.Extensions;
using Suitex.Models;
using Suitex.Services;
using Suitex.ViewModels;
using Suitex.ViewModels.Accounts;

namespace Suitex.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("v1/accounts")]
        public async Task<IActionResult> Post(
            [FromBody] RegisterViewModel model,
            [FromServices] EmailService emailService,
            [FromServices] SuitexDataContext context
            )
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = new User
            {
                // Se quiser a senha na tela model.Password
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email
                    .Replace("@", "-")
                    .Replace(".", "-")
            };

            var password = PasswordGenerator.Generate(25);
            user.PasswordHash = PasswordHasher.Hash(password); // Salvando a senha hasheada
            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                emailService.Send(
                    user.Name,
                    user.Email,
                    subject: "Bem vindo ao SuiteX",
                    body:$@"
Olá {user.Name},<br><br>
Seu acesso foi autorizado.<br>
Login: <strong>{user.Email}</strong><br>
Senha: <strong>{password}</strong>
                    ");

                return Ok(new ResultViewModel<dynamic>(
                    new
                    {
                        user = user.Email, 
                        password
                    }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400,
                    new ResultViewModel<string>("0ACCP01 - Este email já está cadastrado")
                );
            }
            catch
            {
                return StatusCode(500,
                    new ResultViewModel<string>("0ACCE01 - Falha interna no servidor")
                );
            }
        }

        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel model,
            [FromServices] SuitexDataContext context,
            [FromServices] TokenService tokenService
            )
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(
                    ModelState.GetErrors()
                    )
                );

            var user = await context
                .Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));
            
            // PasswordHasher.Hash(senha) -> Gera um novo hash
            
            // PasswordHasher.Verify(senhaHasheada, senhaText) -> Compara senha do banco com senha recebida
            
            if(!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token,null));
            }
            catch
            {
                return StatusCode(500,
                    new ResultViewModel<string>("0ACCE02 - Falha interna no servidor")
                );
            }
        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage(
            [FromBody] UploadImageViewModel model,
            [FromServices] SuitexDataContext context
        )
        {
            var filename = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"data:image\/[a-z]+;base64,")
                .Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{filename}", bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500,
                    new ResultViewModel<string>("0ACCE03 - Falha interna no servidor")
                );
            }

            var user = await context
                .Users
                .FirstOrDefaultAsync(x => User.Identity != null && x.Email == User.Identity.Name);

            if (user == null)
            {
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));
            }

            user.Image = $"https://localhost:7097/images/{filename}"; // Trazer do configuration

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500,
                    new ResultViewModel<string>("0ACCE04 - Falha interna no servidor")
                );
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));
        }
    }
}