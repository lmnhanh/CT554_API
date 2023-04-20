using System.ComponentModel.DataAnnotations;

namespace CT554_API.Models.Auth
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Họ và tên không được trống")]
        public string FullName { get; set; } = null!;
        [Required(ErrorMessage = "Số điện thoại không được trống")]
        public string PhoneNumber { get; set; } = null!;
        [EmailAddress]
        [Required(ErrorMessage = "Email không được trống")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được trống")]
        public string Password { get; set; } = null!;
    }
}
