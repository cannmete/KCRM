using System.ComponentModel.DataAnnotations;

namespace KCRM.ViewModels
{
    public class RegisterViewModel
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = null!;
        
        [Required(ErrorMessage ="E-Posta gereklidir. "), EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
