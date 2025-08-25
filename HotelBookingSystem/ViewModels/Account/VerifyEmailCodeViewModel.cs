using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Account
{
    public class VerifyEmailCodeViewModel
    {
        [Required(ErrorMessage = "Thiếu email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã xác thực")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã phải gồm 6 chữ số")]
        [Display(Name = "Mã xác thực")]
        public string Code { get; set; } = string.Empty;
    }
}
