using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ")]
        [Display(Name = "Họ")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Bạn phải đồng ý với điều khoản sử dụng")]
        [Display(Name = "Đồng ý với điều khoản sử dụng")]
        public bool AcceptTerms { get; set; }
    }
}
