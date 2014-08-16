using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Api.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [DisplayName("User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DisplayName("Remember me")]
        public bool RememberMe { get; set; }
    }
}