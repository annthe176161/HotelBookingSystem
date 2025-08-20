using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Admin
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public DateTime RegisteredDate { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
        public bool IsActivated { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    public class UsersViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
        public UserQueryOptions Query { get; set; } = new();

        public int ActiveUsers { get; set; }
        public int NewUsersLast30Days { get; set; }
        public int BookingUsers { get; set; }
        public int InactiveUsers { get; set; }
    }

    public class UserQueryOptions
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CreateUserViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = default!;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string ConfirmPassword { get; set; } = default!;

        [Display(Name = "Kích hoạt tài khoản")]
        public bool IsActivated { get; set; } = true;

        [Display(Name = "Xác thực email")]
        public bool EmailConfirmed { get; set; } = false;
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = default!;

        [Required, StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Kích hoạt tài khoản")]
        public bool IsActivated { get; set; }

        [Display(Name = "Xác thực email")]
        public bool EmailConfirmed { get; set; }
    }

    public class UserDetailsViewModel
    {
        public EditUserViewModel User { get; set; } = default!;
    }
}
