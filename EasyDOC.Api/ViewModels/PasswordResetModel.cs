using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Api.ViewModels
{
    public class PasswordResetModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}