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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;
using System.Text.Json;
using System.Linq;

namespace HotelBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IImageStorageService _imageStorageService;
        private readonly IEmailService _emailService;
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IImageStorageService imageStorageService,
            IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _imageStorageService = imageStorageService;
            _emailService = emailService;
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

            // UserName: "FirstName LastName" (Title Case, giữ dấu, có khoảng trắng)
            var userNameBase = ToUserName($"{vm.FirstName} {vm.LastName}");
            var userName = await EnsureUniqueUserNameAsync(userNameBase);

            // Gộp FullName từ FirstName + LastName (giữ nguyên dấu & khoảng trắng)
            var fullName = ToUserName($"{vm.FirstName} {vm.LastName}");

            var user = new ApplicationUser
            {
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                UserName = userName,
                FullName = fullName, 
                FirstName = vm.FirstName?.Trim() ?? "",
                LastName = vm.LastName?.Trim() ?? "",
                EmailConfirmed = false // buộc xác thực qua mã
            };

            var create = await _userManager.CreateAsync(user, vm.Password);
            if (!create.Succeeded)
            {
                foreach (var e in create.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            // Lưu claim "đã chấp nhận điều khoản"
            await _userManager.AddClaimAsync(
                user, new System.Security.Claims.Claim("terms.accepted", DateTime.UtcNow.ToString("o"))
            );

            // Gán role mặc định
            const string defaultRole = "Customer";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
                await _roleManager.CreateAsync(new IdentityRole(defaultRole));
            await _userManager.AddToRoleAsync(user, defaultRole);

            // Dùng hàm cấp lớp GenerateNumericCode
            var code = GenerateNumericCode(6);
            var expires = DateTimeOffset.UtcNow.AddMinutes(3);
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(new { c = code, e = expires });

            // Lưu vào AspNetUserTokens (provider "EmailCode", name "email_confirmation_code")
            await _userManager.SetAuthenticationTokenAsync(
                user, "EmailCode", "email_confirmation_code", payloadJson
            );

            // Gửi email
            var subject = "Mã xác thực email - Hotel Booking System";
            var body = $@"<p>Xin chào {(user.FullName ?? user.Email)},</p>
                  <p>Mã xác thực email của bạn là:</p>
                  <h2 style=""letter-spacing:2px;margin:8px 0"">{code}</h2>
                  <p>Mã có hiệu lực trong <strong>3 phút</strong>.</p>
                  <p>Nếu bạn không thực hiện đăng ký, vui lòng bỏ qua email này.</p>";

            await _emailService.SendEmailAsync(user.Email!, subject, body, isHtml: true);

            // KHÔNG đăng nhập ngay; chuyển sang bước nhập mã
            return RedirectToAction(nameof(VerifyEmail), new { email = user.Email });
        }



        // ===== Helpers =====

        private async Task<string> EnsureUniqueUserNameAsync(string baseName)
        {
            var candidate = baseName;
            var i = 0;

            while (await _userManager.FindByNameAsync(candidate) != null)
            {
                i++;
                candidate = $"{baseName} {i}";
            }

            return candidate;
        }

        private static string ToUserName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Chuẩn hóa khoảng trắng: gộp nhiều khoảng trắng thành 1
            var normalized = System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");

            // Viết hoa chữ cái đầu theo văn hoá vi-VN (giữ nguyên dấu tiếng Việt)
            var culture = new System.Globalization.CultureInfo("vi-VN");
            var lower = culture.TextInfo.ToLower(normalized);
            var titled = culture.TextInfo.ToTitleCase(lower);

            return titled;
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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email!);

            // Luôn hiển thị trang xác nhận để tránh lộ thông tin user có tồn tại hay không
            if (user == null)
            {
                return View("ForgotPasswordConfirmation");
            }

            // Tạo token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Tạo link ResetPassword
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { token = encodedToken, email = user.Email },
                Request.Scheme
            )!;

            var subject = "Đặt lại mật khẩu - Hotel Booking System";
            var body = $@"
        <p>Xin chào {(user.FullName ?? user.Email)}</p>
        <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
        <p>Nhấn vào liên kết dưới đây để đặt lại mật khẩu:</p>
        <p><a href=""{callbackUrl}"">Đặt lại mật khẩu</a></p>
        <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

            await _emailService.SendEmailAsync(user.Email!, subject, body, isHtml: true);

            return View("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return BadRequest("Token hoặc email không hợp lệ.");

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email!);
            if (user == null)
            {
                // Không tiết lộ người dùng có tồn tại
                return View("ResetPasswordConfirmation");
            }

            // Decode token
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(vm.Token!);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, vm.Password!);
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(vm);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                // Giữ phiên đăng nhập sau khi đổi mật khẩu
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Mật khẩu đã được cập nhật.";
                return RedirectToAction("ChangePassword"); // hoặc RedirectToAction("Profile")
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider); // chuyển qua Google
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Action("Index", "Home");
            if (!string.IsNullOrEmpty(remoteError))
            {
                TempData["ErrorMessage"] = $"Lỗi đăng nhập ngoài: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Không lấy được thông tin đăng nhập ngoài.";
                return RedirectToAction(nameof(Login));
            }

            // Nếu đã liên kết -> đăng nhập luôn
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (signInResult.Succeeded) return LocalRedirect(returnUrl!);

            // Lấy email + tên từ claims của Google
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var given = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var family = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var nameFromClaim = info.Principal.FindFirstValue(ClaimTypes.Name);
            var displayName = !string.IsNullOrWhiteSpace(nameFromClaim)
                ? nameFromClaim
                : $"{given} {family}".Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Google không trả email. Vui lòng dùng cách đăng nhập khác.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // LẦN ĐẦU: tạo user, lưu FullName
                user = new ApplicationUser
                {
                    UserName = email,          // vẫn để username = email cho unique
                    Email = email,
                    FullName = displayName,    // <-- lưu tên đầy đủ
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var e in createResult.Errors) ModelState.AddModelError("", e.Description);
                    TempData["ErrorMessage"] = "Không thể tạo tài khoản từ Google.";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                // ĐÃ CÓ USER: nếu chưa có tên thì cập nhật
                if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(displayName))
                {
                    user.FullName = displayName;
                    await _userManager.UpdateAsync(user);
                }
            }

            // Liên kết Google login (nếu chưa)
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            // nếu đã liên kết từ trước, AddLoginAsync có thể trả lỗi LoginAlreadyAssociated -> bỏ qua

            await _signInManager.SignInAsync(user, false);
            return LocalRedirect(returnUrl!);
        }

        private static string GenerateNumericCode(int length = 6)
        {
            // Mã 6 số ngẫu nhiên
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            int value = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            int min = (int)Math.Pow(10, length - 1);
            int code = min + (value % (9 * min));
            return code.ToString();
        }

        private async Task SendEmailVerificationCodeAsync(ApplicationUser user)
        {
            var code = GenerateNumericCode(6);
            var expires = DateTimeOffset.UtcNow.AddMinutes(3);
            var payload = JsonSerializer.Serialize(new { c = code, e = expires });

            // Lưu mã vào AspNetUserTokens (provider/purpose tùy chọn)
            await _userManager.SetAuthenticationTokenAsync(
                user, "EmailCode", "email_confirmation_code", payload);

            var subject = "Mã xác thực email - Hotel Booking System";
            var body = $@"<p>Xin chào {(user.FullName ?? user.Email)}</p>
                  <p>Mã xác thực email của bạn là:</p>
                  <h2 style=""letter-spacing:2px"">{code}</h2>
                  <p>Mã có hiệu lực trong 10 phút.</p>
                  <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>";

            await _emailService.SendEmailAsync(user.Email!, subject, body, isHtml: true);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Login));
            return View(new VerifyEmailCodeViewModel { Email = email });
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailCodeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Email không tồn tại.";
                return View(model);
            }

            var tokenJson = await _userManager.GetAuthenticationTokenAsync(
                user, "EmailCode", "email_confirmation_code");

            if (string.IsNullOrWhiteSpace(tokenJson))
            {
                TempData["ErrorMessage"] = "Mã không hợp lệ hoặc đã hết hạn. Vui lòng bấm 'Gửi lại mã'.";
                return View(model);
            }

            try
            {
                var payload = System.Text.Json.JsonSerializer.Deserialize<EmailCodePayload>(tokenJson);
                if (payload == null || payload.c != model.Code)
                {
                    TempData["ErrorMessage"] = "Mã xác thực không chính xác.";
                    return View(model);
                }
                if (DateTimeOffset.UtcNow > payload.e)
                {
                    TempData["ErrorMessage"] = "Mã đã hết hạn. Vui lòng bấm 'Gửi lại mã'.";
                    return View(model);
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Mã không hợp lệ.";
                return View(model);
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _userManager.RemoveAuthenticationTokenAsync(
                user, "EmailCode", "email_confirmation_code");

            TempData["SuccessMessage"] = "Xác thực email thành công. Bạn có thể đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailCode(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Login));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return RedirectToAction(nameof(Login));

            if (user.EmailConfirmed)
            {
                TempData["SuccessMessage"] = "Email đã được xác thực trước đó.";
                return RedirectToAction(nameof(Login));
            }

            await SendEmailVerificationCodeAsync(user);
            TempData["SuccessMessage"] = "Đã gửi lại mã xác thực.";
            return RedirectToAction(nameof(VerifyEmail), new { email });
        }

        // model record để deserialize token json (đặt cùng vùng helper nếu muốn)
        private record EmailCodePayload(string c, DateTimeOffset e);

        #region Helper Methods

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.City = model.City;
                user.State = model.State;
                user.ZipCode = model.ZipCode;
                user.DateOfBirth = model.Birthdate;

                // Handle gender conversion
                if (!string.IsNullOrEmpty(model.Gender))
                {
                    user.GenderType = model.Gender.ToLower() switch
                    {
                        "male" => GenderType.Name,
                        "female" => GenderType.Nu,
                        _ => GenderType.Unknow
                    };
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            // Always redirect back to GET to reload full data
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ảnh để upload." });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng." });
                }

                // Delete old avatar if exists
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    try
                    {
                        // Extract public ID from old URL if it's a Cloudinary URL
                        var oldPublicId = ExtractPublicIdFromUrl(user.ProfilePictureUrl);
                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _imageStorageService.Delete(oldPublicId);
                        }
                    }
                    catch
                    {
                        // Ignore delete errors for old avatar
                    }
                }

                // Upload new avatar
                var uploadResult = await _imageStorageService.UploadAvatarImage(avatar);

                // Update user profile picture URL
                user.ProfilePictureUrl = uploadResult.Url;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Cập nhật ảnh đại diện thành công!",
                        avatarUrl = uploadResult.Url
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật ảnh đại diện." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        private string? ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                // Extract public ID from Cloudinary URL
                // Format: https://res.cloudinary.com/cloud/image/upload/v1234567890/folder/public_id.format
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');

                if (segments.Length >= 3)
                {
                    // Find the segment after 'upload' and extract public ID
                    var uploadIndex = Array.IndexOf(segments, "upload");
                    if (uploadIndex != -1 && uploadIndex + 2 < segments.Length)
                    {
                        // Skip version (v1234567890) if present
                        var startIndex = segments[uploadIndex + 1].StartsWith("v") ? uploadIndex + 2 : uploadIndex + 1;
                        var pathParts = segments.Skip(startIndex).ToArray();
                        var publicIdWithExtension = string.Join("/", pathParts);

                        // Remove file extension
                        var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
                        if (lastDotIndex > 0)
                        {
                            return publicIdWithExtension.Substring(0, lastDotIndex);
                        }
                        return publicIdWithExtension;
                    }
                }
            }
            catch
            {
                // Return null if URL parsing fails
            }
            return null;
        }

        #endregion
    }

}

