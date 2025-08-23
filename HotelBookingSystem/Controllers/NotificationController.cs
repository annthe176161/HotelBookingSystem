using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingSystem.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Thông báo của tôi";
            return View();
        }
    }
}
