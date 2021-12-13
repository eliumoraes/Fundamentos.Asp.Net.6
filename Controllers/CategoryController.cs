using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Suitex.Data;
using Suitex.Extensions;
using Suitex.Models;
using Suitex.ViewModels;
using Suitex.ViewModels.Categories;

namespace Suitex.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        // Trabalhando com versionamento de endpoint
        //localhost:port/v1/categories
        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices] IMemoryCache cache,
            [FromServices] SuitexDataContext context)
        {
            try
            {
                // var categories = await context.Categories.ToListAsync();
                var categories = cache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });
                return Ok(
                    new ResultViewModel<List<Category>>(categories)
                    );
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return StatusCode(500,
                    new ResultViewModel<List<Category>>("0CTYE01 - Falha interna no servidor")
                    );
            }
        }

        // Método chamado quando não possui cache
        private List<Category> GetCategories(SuitexDataContext context)
        {
            return context.Categories.ToList();
        }

        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id,
            [FromServices] SuitexDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(
                x => x.Id == id);

                if (category == null)
                    return NotFound(
                        new ResultViewModel<Category>("Categoria não encontrada!")
                    );

                return Ok(
                    new ResultViewModel<Category>(category)
                    );
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return StatusCode(500, new ResultViewModel<Category>(
                    "0CTYE02 - Falha interna no servidor"));
            }
        }

        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorCategoryViewModel model,
            [FromServices] SuitexDataContext context)
        {
            // Não está mais usando o DataAnottations
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ResultViewModel<Category>(ModelState.GetErrors())
                    );
            }
            try
            {
                var category = new Category
                {
                    Id = 0,
                    Name = model.Name,
                    Slug = model.Slug.ToLower()
                };
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return Created($"v1/categories/{category.Id}", new
                    ResultViewModel<Category>(category)
                );
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return StatusCode(500, new
                    ResultViewModel<Category>("0CTYP01 - Não foi possível incluir a categoria")
                );
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return StatusCode(500, new
                    ResultViewModel<Category>("0CTYE03 - Falha interna no servidor")
                );
            }
        }

        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EditorCategoryViewModel model,
            [FromServices] SuitexDataContext context)
        {
            try
            {
                var category = await context
                .Categories
                .FirstOrDefaultAsync(x => x.Id == id);

                if (category == null)
                    return NotFound(new
                    ResultViewModel<Category>("Categoria não encontrada"));

                category.Name = model.Name;
                category.Slug = model.Slug.ToLower();

                context.Categories.Update(category);
                await context.SaveChangesAsync();

                return Ok(new
                    ResultViewModel<Category>(category)
                );
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return StatusCode(500, new
                    ResultViewModel<Category>("0CTYP02 - Não foi possível atualizar a categoria"
                ));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return StatusCode(500, new
                    ResultViewModel<Category>("0CTYE04 - Não foi possível atualizar a categoria"
                ));
            }

        }

        [HttpDelete("/v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] int id,
            [FromServices] SuitexDataContext context)
        {
            try
            {
                var category = await context
                .Categories
                .FirstOrDefaultAsync(x => x.Id == id);

                if (category == null)
                    return NotFound(new
                        ResultViewModel<Category>("Categoria não encontrada"
                    ));

                context.Categories.Remove(category);
                await context.SaveChangesAsync();

                return Ok(new
                    ResultViewModel<Category>(category));
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return StatusCode(500, new
                    ResultViewModel<Category>("0CTYP03 - Não foi possível incluir a categoria"
                ));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return StatusCode(500, new
                    ResultViewModel<Category>("0CTYE05 - Não foi possível atualizar a categoria"
                ));
            }
        }
    }
}
