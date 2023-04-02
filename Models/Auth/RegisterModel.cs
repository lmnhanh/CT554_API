using System.ComponentModel.DataAnnotations;

namespace CT554_API.Models.Auth
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; } = null!;

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }
}
