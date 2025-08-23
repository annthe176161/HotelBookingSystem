using HotelBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly INotificationService _notificationService;

        public TestController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestNotification()
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    await _notificationService.SendGeneralNotificationToAdminsAsync(
                        "Test notification for admin - " + DateTime.Now.ToString("HH:mm:ss"),
                        "info"
                    );
                }
                else
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _notificationService.SendGeneralNotificationToUserAsync(
                            userId,
                            "Test notification for user - " + DateTime.Now.ToString("HH:mm:ss"),
                            "info"
                        );
                    }
                }

                return Json(new { success = true, message = "Test notification sent!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendTestBookingNotification()
        {
            try
            {
                await _notificationService.SendBookingNotificationToAdminAsync(
                    "TEST123",
                    "Test Customer",
                    "Test Room",
                    $"Test booking notification - {DateTime.Now:HH:mm:ss}"
                );

                return Json(new { success = true, message = "Test booking notification sent to admins!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestAdminNotification()
        {
            try
            {
                Console.WriteLine($"[DEBUG] TestAdminNotification called by user: {User.Identity.Name}");

                await _notificationService.SendBookingNotificationToAdminAsync(
                    "DEBUG-" + Guid.NewGuid().ToString()[..8],
                    "Debug Customer",
                    "Debug Room",
                    $"DEBUG: Admin notification test at {DateTime.Now:HH:mm:ss}"
                );

                return Json(new { success = true, message = "Debug admin notification sent!", timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] TestAdminNotification error: {ex.Message}");
                return Json(new { success = false, message = ex.Message, error = ex.ToString() });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestCustomerNotification()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"[DEBUG] TestCustomerNotification called by user: {User.Identity.Name}, UserId: {userId}");

                if (!string.IsNullOrEmpty(userId))
                {
                    // Test booking status update notification
                    await _notificationService.SendBookingStatusUpdateToCustomerAsync(
                        userId,
                        "TEST-001",
                        "Đã xác nhận",
                        $"TEST: Đặt phòng TEST-001 đã được xác nhận - {DateTime.Now:HH:mm:ss}"
                    );

                    return Json(new { success = true, message = "Test customer notification sent!", userId = userId, timestamp = DateTime.Now });
                }

                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] TestCustomerNotification error: {ex.Message}");
                return Json(new { success = false, message = ex.Message, error = ex.ToString() });
            }
        }
    }
}
