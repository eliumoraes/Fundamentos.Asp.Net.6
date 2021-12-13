using System.ComponentModel.DataAnnotations;

namespace Suitex.ViewModels.Accounts
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "O email é inválido")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; }
    }
}