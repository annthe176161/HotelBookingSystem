using HotelBookingSystem.Data;
using HotelBookingSystem.Infrastructure.Filters;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Room;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace HotelBookingSystem.Controllers
{
    [SkipAccountActiveCheck]
    public class HomeController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(
            IRoomService roomService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _roomService = roomService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra trạng thái tài khoản nếu user đã đăng nhập
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null || !user.IsActivated)
                    {
                        // Đăng xuất và redirect
                        await _signInManager.SignOutAsync();
                        HttpContext.Session.Clear();
                        
                        // Xóa authentication cookies
                        foreach (var cookie in Request.Cookies.Keys)
                        {
                            if (cookie.StartsWith(".AspNetCore") || cookie.Contains("Identity"))
                            {
                                Response.Cookies.Delete(cookie);
                            }
                        }
                        
                        TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin để được hỗ trợ.";
                        return RedirectToAction("Login", "Account", new { message = "account_deactivated" });
                    }
                }
            }
            
            var featuredRooms = await _roomService.GetFeaturedRoomsAsync(4);
            return View(featuredRooms);
        }

        public async Task<IActionResult> Rooms(RoomListViewModel searchModel, int page = 1)
        {
            // Set the current page from parameter
            searchModel.CurrentPage = page;

            // Normalize empty strings to null for proper filtering
            if (string.IsNullOrWhiteSpace(searchModel.RoomType))
            {
                searchModel.RoomType = null;
            }

            // The controller just calls the service. All logic is in the service.
            var viewModel = await _roomService.GetFilteredRoomsAsync(searchModel);
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
