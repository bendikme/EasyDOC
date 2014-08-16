using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Api.ViewModels
{
    public class NewPasswordModel
    {
        public string Token { get; set; }

        [Required]
        [StringLength(99, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The passwords must match")]
        public string ConfirmPassword { get; set; }
    }
}