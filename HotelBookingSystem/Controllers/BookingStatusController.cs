using HotelBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HotelBookingSystem.Controllers
{
    [Authorize]
    public class BookingStatusController : Controller
    {
        private readonly IBookingStatusService _bookingStatusService;
        private readonly ILogger<BookingStatusController> _logger;

        public BookingStatusController(
            IBookingStatusService bookingStatusService,
            ILogger<BookingStatusController> logger)
        {
            _bookingStatusService = bookingStatusService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, int newStatusId, string reason = "")
        {
            try
            {
                await _bookingStatusService.UpdateBookingStatusAsync(bookingId, newStatusId, reason);
                TempData["Success"] = "Cập nhật trạng thái đặt phòng thành công và đã gửi email thông báo!";
                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating booking status for booking {bookingId}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int bookingId, int newPaymentStatusId, string reason = "")
        {
            try
            {
                await _bookingStatusService.UpdatePaymentStatusAsync(bookingId, newPaymentStatusId, reason);
                TempData["Success"] = "Cập nhật trạng thái thanh toán thành công và đã gửi email thông báo!";
                return Json(new { success = true, message = "Cập nhật trạng thái thanh toán thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating payment status for booking {bookingId}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId, string reason)
        {
            try
            {
                await _bookingStatusService.CancelBookingAsync(bookingId, reason);
                TempData["Success"] = "Hủy đặt phòng thành công và đã gửi email thông báo!";
                return Json(new { success = true, message = "Hủy đặt phòng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling booking {bookingId}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendCheckInReminder(int bookingId)
        {
            try
            {
                await _bookingStatusService.SendCheckInReminderAsync(bookingId);
                TempData["Success"] = "Gửi email nhắc nhở check-in thành công!";
                return Json(new { success = true, message = "Gửi email nhắc nhở thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending check-in reminder for booking {bookingId}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendPaymentReminder(int bookingId)
        {
            try
            {
                await _bookingStatusService.SendPaymentReminderAsync(bookingId);
                TempData["Success"] = "Gửi email nhắc nhở thanh toán thành công!";
                return Json(new { success = true, message = "Gửi email nhắc nhở thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending payment reminder for booking {bookingId}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API để test gửi email
        [HttpGet]
        public IActionResult TestEmailPage()
        {
            return View();
        }
    }
}
