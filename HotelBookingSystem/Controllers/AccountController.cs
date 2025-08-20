using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            // Đăng nhập bằng EMAIL + PASSWORD
            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user is null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Nếu có returnUrl (đi từ trang đặt phòng) thì quay lại đó, ngược lại về Home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                ModelState.AddModelError("", "Tài khoản bị khóa tạm thời.");
            else if (result.IsNotAllowed)
                ModelState.AddModelError("", "Tài khoản chưa được phép đăng nhập.");
            else
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");

            return View(vm);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Bắt buộc đồng ý điều khoản
            if (!vm.AcceptTerms)
                ModelState.AddModelError(nameof(vm.AcceptTerms), "Bạn phải đồng ý với điều khoản sử dụng");

            if (!ModelState.IsValid) return View(vm);

            // Email đã tồn tại?
            if (await _userManager.FindByEmailAsync(vm.Email) is not null)
            {
                ModelState.AddModelError(nameof(vm.Email), "Email đã được sử dụng");
                return View(vm);
            }

            // Tạo UserName từ FirstName + LastName (bỏ dấu, liền nhau, thường)
            var userNameBase = ToUserName($"{vm.FirstName}{vm.LastName}");
            var userName = await EnsureUniqueUserNameAsync(userNameBase);

            // Gộp FullName từ FirstName + LastName
            var fullName = $"{vm.FirstName?.Trim()} {vm.LastName?.Trim()}".Trim();

            var user = new ApplicationUser
            {
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                UserName = userName,
                FullName = fullName
            };

            var create = await _userManager.CreateAsync(user, vm.Password);
            if (!create.Succeeded)
            {
                foreach (var e in create.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            // Lưu claim "đã chấp nhận điều khoản"
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("terms.accepted", DateTime.UtcNow.ToString("o")));

            // Gán role mặc định
            const string defaultRole = "Customer";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
                await _roleManager.CreateAsync(new IdentityRole(defaultRole));
            await _userManager.AddToRoleAsync(user, defaultRole);

            // Đăng nhập ngay (nếu bạn đặt RequireConfirmedEmail = true thì bỏ bước này)
            await _signInManager.SignInAsync(user, isPersistent: true);

            return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
        }

        // ===== Helpers =====

        private async Task<string> EnsureUniqueUserNameAsync(string baseName)
        {
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "user";
            var candidate = baseName;
            var i = 0;
            while (await _userManager.FindByNameAsync(candidate) != null)
            {
                i++;
                candidate = $"{baseName}{i}";
            }
            return candidate;
        }

        private static string ToUserName(string input)
        {
            // Bỏ dấu tiếng Việt, giữ chữ/số, về lowercase
            input = RemoveDiacritics(input ?? string.Empty).Trim();
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
                if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            return sb.ToString();
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = (text ?? string.Empty).Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //public IActionResult Profile()
        //{
        //    // Tạo dữ liệu mẫu cho trang profile
        //    var model = new ProfileViewModel
        //    {
        //        FirstName = "Nguyễn",
        //        LastName = "Văn A",
        //        Email = "nguyenvana@gmail.com",
        //        PhoneNumber = "0987654321",
        //        Birthdate = new DateTime(1990, 1, 1),
        //        Gender = "male",
        //        Address = "123 Nguyễn Văn Linh",
        //        City = "Hồ Chí Minh",
        //        State = "TP.HCM",
        //        ZipCode = "70000",
        //        CreatedAt = DateTime.Now.AddMonths(-6),
        //        BookingsCount = 5,
        //        ReviewsCount = 3,
        //        LoyaltyPoints = 250,
        //        LoyaltyTier = "Silver",
        //        NextTier = "Gold",
        //        PointsToNextTier = 150,
        //        NextTierProgress = 62,
        //        TwoFactorEnabled = false,

        //        // Thêm dữ liệu đặt phòng mẫu
        //        Bookings = new List<BookingItemViewModel>
        //        {
        //            new BookingItemViewModel
        //            {
        //                Id = 1,
        //                BookingNumber = "BK001",
        //                RoomName = "Phòng Deluxe Hướng Biển",
        //                RoomImageUrl = "/images/rooms/deluxe-ocean.jpg",
        //                CheckInDate = DateTime.Now.AddDays(14),
        //                CheckOutDate = DateTime.Now.AddDays(17),
        //                GuestsCount = 2,
        //                TotalPrice = 6000000,
        //                Status = "Confirmed",
        //                BookingDate = DateTime.Now.AddDays(-10),
        //                HasReview = false
        //            },
        //            new BookingItemViewModel
        //            {
        //                Id = 2,
        //                BookingNumber = "BK002",
        //                RoomName = "Phòng Suite Gia Đình",
        //                RoomImageUrl = "/images/rooms/family-suite.jpg",
        //                CheckInDate = DateTime.Now.AddDays(-14),
        //                CheckOutDate = DateTime.Now.AddDays(-10),
        //                GuestsCount = 4,
        //                TotalPrice = 14000000,
        //                Status = "Completed",
        //                BookingDate = DateTime.Now.AddDays(-25),
        //                HasReview = true
        //            }
        //        },

        //        // Thêm đánh giá mẫu
        //        Reviews = new List<ReviewItemViewModel>
        //        {
        //            new ReviewItemViewModel
        //            {
        //                Id = 1,
        //                RoomName = "Phòng Suite Gia Đình",
        //                Rating = 5,
        //                Comment = "Phòng rất tuyệt vời, rộng rãi và sạch sẽ. Nhân viên thân thiện và nhiệt tình. Sẽ quay lại lần sau!",
        //                CreatedAt = DateTime.Now.AddDays(-9)
        //            },
        //            new ReviewItemViewModel
        //            {
        //                Id = 2,
        //                RoomName = "Phòng Executive Business",
        //                Rating = 4,
        //                Comment = "Phòng tốt, đầy đủ tiện nghi cho chuyến công tác. Chỉ tiếc là wifi hơi chậm.",
        //                CreatedAt = DateTime.Now.AddDays(-45)
        //            }
        //        },

        //        // Thêm phòng yêu thích mẫu
        //        FavoriteRooms = new List<FavoriteRoomViewModel>
        //        {
        //            new FavoriteRoomViewModel
        //            {
        //                Id = 1,
        //                Name = "Phòng Tổng Thống",
        //                ImageUrl = "/images/rooms/presidential-suite.jpg",
        //                Price = 8000000,
        //                Rating = 5.0,
        //                Capacity = 2,
        //                RoomType = "Presidential"
        //            },
        //            new FavoriteRoomViewModel
        //            {
        //                Id = 2,
        //                Name = "Phòng Deluxe Hướng Biển",
        //                ImageUrl = "/images/rooms/deluxe-ocean.jpg",
        //                Price = 2000000,
        //                Rating = 4.5,
        //                Capacity = 2,
        //                RoomType = "Deluxe"
        //            }
        //        },
            
        //        // Thêm lịch sử điểm thưởng mẫu
        //        LoyaltyPointsHistory = new List<LoyaltyPointHistoryViewModel>
        //        {
        //            new LoyaltyPointHistoryViewModel
        //            {
        //                Date = DateTime.Now.AddDays(-10),
        //                Description = "Đặt phòng BK001",
        //                Points = 60,
        //                Status = "Completed"
        //            },
        //            new LoyaltyPointHistoryViewModel
        //            {
        //                Date = DateTime.Now.AddDays(-25),
        //                Description = "Đặt phòng BK002",
        //                Points = 140,
        //                Status = "Completed"
        //            },
        //            new LoyaltyPointHistoryViewModel
        //            {
        //                Date = DateTime.Now.AddDays(-60),
        //                Description = "Điểm thưởng chào mừng",
        //                Points = 50,
        //                Status = "Completed"
        //            }
        //        },

        //        // Thêm hoạt động đăng nhập mẫu
        //        LoginActivities = new List<LoginActivityViewModel>
        //        {
        //            new LoginActivityViewModel
        //            {
        //                Device = "Windows Chrome 115.0.5790",
        //                Location = "Hồ Chí Minh, Việt Nam",
        //                IpAddress = "203.113.148.XX",
        //                Time = DateTime.Now.AddHours(-1),
        //                Status = "Current"
        //            },
        //            new LoginActivityViewModel
        //            {
        //                Device = "iPhone Safari iOS 16.5",
        //                Location = "Hồ Chí Minh, Việt Nam",
        //                IpAddress = "42.116.83.XX",
        ////                Time = DateTime.Now.AddDays(-2),[HttpGet]
        //public async Task<IActionResult> Profile()
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    // Tách FirstName / LastName từ FullName nếu DB chỉ có FullName
        //    var (first, last) = SplitFullName(user.FullName);

        //    var vm = new ProfileViewModel
        //    {
        //        FirstName = first,
        //        LastName = last,
        //        Email = user.Email ?? "",
        //        PhoneNumber = user.PhoneNumber ?? "",

        //        // Nếu ApplicationUser CHƯA có các cột bên dưới, hãy để trống hoặc lấy từ nơi khác
        //        // BirthDate  = user.BirthDate,
        //        // Gender     = user.Gender,
        //        // Address    = user.Address,
        //        // City       = user.City,
        //        // State      = user.State,
        //        // ZipCode    = user.ZipCode,

        //        // Các con số hiển thị bên trái (nếu bạn có bảng Booking/Review):
        //        BookingCount = await _db.Bookings.CountAsync(b => b.UserId == user.Id),
        //        ReviewCount = await _db.Reviews.CountAsync(r => r.UserId == user.Id),
        //        RewardPoints = 0 // nếu có cột/logic điểm thưởng thì thay ở đây
        //    };

        //    return View(vm); // View Profile.cshtml của bạn sẽ tự fill các ô
        //}
        //                Status = "Success"
        //            },
        //            new LoginActivityViewModel
        //            {
        //                Device = "Android Chrome 115.0.5790",
        //                Location = "Hà Nội, Việt Nam",
        //                IpAddress = "14.241.224.XX",
        //                Time = DateTime.Now.AddDays(-7),
        //                Status = "Success"
        //            }
        //        }
        //    };

        //    return View(model);
        //}

        

        // Helper tách họ tên
        private static (string first, string last) SplitFullName(string? fullName)
        {
            fullName = (fullName ?? "").Trim();
            if (string.IsNullOrEmpty(fullName)) return ("", "");
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return (parts[0], "");
            return (string.Join(' ', parts[..^1]), parts[^1]); // first = tất cả trừ từ cuối, last = từ cuối
        }
    }

}

