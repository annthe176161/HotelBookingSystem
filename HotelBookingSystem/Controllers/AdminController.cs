using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Implementations;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HotelBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminRoomService _adminRoomService;
        private readonly IAdminUserService _adminUserService;
        private readonly IAdminBookingService _adminBookingService;
        private readonly IAdminReviewService _adminReviewService;

        public AdminController(IAdminRoomService adminRoomService, IAdminUserService adminUserService, IAdminBookingService adminBookingService, IAdminReviewService adminReviewService)
        {
            _adminRoomService = adminRoomService;
            _adminUserService = adminUserService;
            _adminBookingService = adminBookingService;
            _adminReviewService = adminReviewService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            // Chuyển hướng đến Dashboard
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            // Trả về giao diện Dashboard
            return View("Dashboard");
        }

        [HttpGet("Rooms")]
        public async Task<IActionResult> Rooms([FromQuery] RoomsQueryViewModel query)
        {
            var result = await _adminRoomService.GetRooms(query);

            return View("AdminRooms", result);
        }

        [HttpPost("AddRoom")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(CreateRoomViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _adminRoomService.Add(model, ct);
                return RedirectToAction("Rooms");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi tạo phòng: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost("Deactivate/{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _adminRoomService.DeactivateRoom(id);
            return RedirectToAction("Rooms");
        }

        [HttpPost("Activate/{id}")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _adminRoomService.ActivateRoom(id);
            return RedirectToAction("Rooms");
        }

        [HttpGet("EditRoom/{id}")]
        public async Task<IActionResult> EditRoom(int id, CancellationToken ct)
        {
            var vm = await _adminRoomService.GetById(id, ct);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost("EditRoom/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, RoomDetailsViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updatedRoom = await _adminRoomService.Update(model.Room, ct);
                if (updatedRoom == null) return NotFound();

                return RedirectToAction("Rooms");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi cập nhật phòng: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet("AddUser")]
        public IActionResult AddUser()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost("AddUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(CreateUserViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _adminUserService.Add(model, ct);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            return RedirectToAction("Users");
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users([FromQuery] UserQueryOptions options)
        {
            var result = await _adminUserService.GetUsers(options);
            return View("AdminUsers", result);
        }

        [HttpGet("EditUser/{id}")]
        public async Task<IActionResult> EditUser(string id, CancellationToken ct)
        {
            var vm = await _adminUserService.GetById(id, ct);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost("EditUser/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserDetailsViewModel wrapper, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(wrapper);

            var result = await _adminUserService.Update(wrapper.User, ct);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(wrapper);
            }

            return RedirectToAction("Users");
        }

        [HttpGet("Bookings")]
        public async Task<IActionResult> Bookings([FromQuery] BookingQueryOptions query)
        {
            var model = await _adminBookingService.GetBookings(query);
            return View("AdminBookings", model);
        }

        [HttpPost]
        [Route("Admin/UpdateBookingStatus")]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var result = await _adminBookingService.UpdateBookingStatus(id, status);

            if (!result.success)
            {
                TempData["Error"] = result.message;
            }
            else
            {
                TempData["Success"] = result.message;
            }

            // Always redirect back to Bookings page after action
            return RedirectToAction("Bookings");
        }

        [HttpPost]
        [Route("Admin/UpdatePaymentStatus")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, string paymentStatus)
        {
            var result = await _adminBookingService.UpdatePaymentStatus(id, paymentStatus);

            if (!result)
            {
                TempData["Error"] = "Không thể cập nhật trạng thái thanh toán.";
            }
            else
            {
                TempData["Success"] = "Cập nhật trạng thái thanh toán thành công.";
            }

            // Always redirect back to Bookings page after action
            return RedirectToAction("Bookings");
        }

        [HttpGet("Promotions")]
        public IActionResult Promotions()
        {
            // Trả về giao diện quản lý khuyến mãi
            return View("AdminPromotions");
        }

        [HttpGet("Reports")]
        public IActionResult Reports()
        {
            // Trả về giao diện báo cáo tổng hợp
            return View("AdminReports");
        }

        [HttpGet("Settings")]
        public IActionResult Settings()
        {
            // Trả về giao diện cài đặt admin
            return View("AdminSettings");
        }

        [HttpGet("AddRoom")]
        public IActionResult AddRoom()
        {
            return View();
        }

        [HttpGet("Reviews")]
        public async Task<IActionResult> Reviews([FromQuery] ReviewsQueryViewModel query)
        {
            var result = await _adminReviewService.GetReviews(query);

            return View("AdminReviews", result);
        }
    }
}
