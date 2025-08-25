using Microsoft.AspNetCore.Identity;
using HotelBookingSystem.Models;
using System.Security.Claims;

namespace HotelBookingSystem.Infrastructure.Middleware
{
    public class AccountStatusCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AccountStatusCheckMiddleware> _logger;

        public AccountStatusCheckMiddleware(RequestDelegate next, ILogger<AccountStatusCheckMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            // Bỏ qua kiểm tra cho những request không cần thiết
            if (ShouldSkipCheck(context))
            {
                await _next(context);
                return;
            }

            // Kiểm tra nếu user đã đăng nhập
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);
                    
                    // Kiểm tra nếu tài khoản bị khóa hoặc không tồn tại
                    if (user == null || !user.IsActivated)
                    {
                        _logger.LogWarning($"ACCOUNT LOCKED: User {userId} ({user?.Email}) attempted to access with deactivated account. Path: {context.Request.Path}");
                        
                        // Đăng xuất user
                        await signInManager.SignOutAsync();
                        
                        // Xóa session và cookies
                        context.Session.Clear();
                        
                        // Xóa thêm các authentication cookies khác
                        foreach (var cookie in context.Request.Cookies.Keys)
                        {
                            if (cookie.StartsWith(".AspNetCore") || cookie.Contains("Identity"))
                            {
                                context.Response.Cookies.Delete(cookie);
                            }
                        }
                        
                        // Kiểm tra nếu là AJAX request
                        if (IsAjaxRequest(context))
                        {
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"error\":\"Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin để được hỗ trợ.\",\"redirectUrl\":\"/Account/Login\"}");
                            return;
                        }
                        
                        // Redirect với thông báo
                        context.Response.Redirect("/Account/Login?message=account_deactivated");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private static bool ShouldSkipCheck(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            
            // Bỏ qua các đường dẫn không cần kiểm tra
            var skipPaths = new[]
            {
                "/account/login",
                "/account/logout", 
                "/account/register",
                "/account/forgotpassword",
                "/account/resetpassword",
                "/account/confirmemail",
                "/account/accessdenied",
                "/account/externallogincallback", // Thêm callback Google để xử lý trong controller
                "/home/error",
                "/home/privacy",
                "/css/",
                "/js/",
                "/images/",
                "/lib/",
                "/favicon.ico",
                "/microsoft/",
                "/notificationhub"
            };

            // KHÔNG bỏ qua trang chủ "/" và "/home/index" nếu user đã authenticated
            // Điều này đảm bảo user bị khóa sẽ được kiểm tra ngay khi vào trang chủ
            
            return skipPaths.Any(skipPath => path?.StartsWith(skipPath) == true);
        }

        private static bool IsAjaxRequest(HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   context.Request.Headers["Content-Type"].ToString().Contains("application/json") ||
                   context.Request.Path.StartsWithSegments("/api");
        }
    }
}
