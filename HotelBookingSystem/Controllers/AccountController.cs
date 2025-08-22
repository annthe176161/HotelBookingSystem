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
using Microsoft.EntityFrameworkCore;

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
                // Kiểm tra role để chuyển hướng phù hợp
                var roles = await _userManager.GetRolesAsync(user);

                // Nếu có returnUrl (đi từ trang đặt phòng) thì quay lại đó
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                // Nếu là admin thì chuyển đến admin dashboard
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                // Người dùng thường thì về Home
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
                FullName = fullName,
                FirstName = vm.FirstName?.Trim() ?? "",
                LastName = vm.LastName?.Trim() ?? "",
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
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Tách FirstName / LastName từ FullName nếu có
            var firstName = user.FirstName ?? "";
            var lastName = user.LastName ?? "";

            if (string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(user.FullName))
            {
                var nameParts = user.FullName.Split(' ', 2);
                firstName = nameParts.Length > 0 ? nameParts[0] : "";
                lastName = nameParts.Length > 1 ? nameParts[1] : "";
            }

            var vm = new ProfileViewModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Birthdate = user.DateOfBirth ?? DateTime.Now.AddYears(-25),
                Gender = user.Gender == GenderType.Name ? "male" :
                        user.Gender == GenderType.Nu ? "female" : "other",
                Address = user.Address ?? "",
                City = user.City ?? "",
                State = user.State ?? "",
                ZipCode = user.ZipCode ?? "",
                AvatarUrl = user.ProfilePictureUrl ?? "/images/default-avatar.png",
                CreatedAt = user.CreatedAt,

                // Đếm số booking và review nếu có
                BookingsCount = await _db.Bookings.CountAsync(b => b.UserId == user.Id),
                ReviewsCount = await _db.Reviews.CountAsync(r => r.UserId == user.Id),

                // Mock data cho loyalty system
                LoyaltyPoints = 250,
                LoyaltyTier = "Silver",
                NextTier = "Gold",
                PointsToNextTier = 150,
                NextTierProgress = 62,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user)
            };

            return View(vm);
        }

        #region Helper Methods

        // Helper tách họ tên
        private static (string first, string last) SplitFullName(string? fullName)
        {
            fullName = (fullName ?? "").Trim();
            if (string.IsNullOrEmpty(fullName)) return ("", "");
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return (parts[0], "");
            return (string.Join(' ', parts[..^1]), parts[^1]); // first = tất cả trừ từ cuối, last = từ cuối
        }

        #endregion
    }
}

