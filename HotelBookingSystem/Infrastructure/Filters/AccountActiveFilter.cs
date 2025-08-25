using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using HotelBookingSystem.Models;
using System.Security.Claims;

namespace HotelBookingSystem.Infrastructure.Filters
{
    public class AccountActiveFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountActiveFilter> _logger;

        public AccountActiveFilter(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountActiveFilter> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Kiểm tra nếu có attribute SkipAccountActiveCheck
            if (context.ActionDescriptor.EndpointMetadata.Any(m => m is SkipAccountActiveCheckAttribute) ||
                context.Controller.GetType().GetCustomAttributes(typeof(SkipAccountActiveCheckAttribute), true).Any())
            {
                await next();
                return;
            }

            // Chỉ kiểm tra cho authenticated users
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    
                    if (user == null || !user.IsActivated)
                    {
                        _logger.LogWarning($"FILTER BLOCK: User {userId} ({user?.Email}) attempted to access {context.ActionDescriptor.DisplayName} with deactivated account.");
                        
                        // Đăng xuất
                        await _signInManager.SignOutAsync();
                        context.HttpContext.Session.Clear();

                        // Xóa authentication cookies
                        foreach (var cookie in context.HttpContext.Request.Cookies.Keys)
                        {
                            if (cookie.StartsWith(".AspNetCore") || cookie.Contains("Identity"))
                            {
                                context.HttpContext.Response.Cookies.Delete(cookie);
                            }
                        }

                        // Kiểm tra nếu là AJAX request
                        if (IsAjaxRequest(context.HttpContext))
                        {
                            context.Result = new JsonResult(new 
                            { 
                                error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin để được hỗ trợ.",
                                redirectUrl = "/Account/Login?message=account_deactivated"
                            })
                            {
                                StatusCode = 401
                            };
                        }
                        else
                        {
                            context.Result = new RedirectResult("/Account/Login?message=account_deactivated");
                        }
                        
                        return;
                    }
                }
            }

            await next();
        }

        private static bool IsAjaxRequest(HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   context.Request.Headers["Content-Type"].ToString().Contains("application/json") ||
                   context.Request.Path.StartsWithSegments("/api");
        }
    }
}
