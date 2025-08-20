using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HotelBookingSystem.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            // Trả về giao diện Dashboard
            return View("Dashboard");
        }

        [HttpGet("Bookings")]
        public IActionResult Bookings()
        {
            // Trả về giao diện quản lý đặt phòng
            return View("AdminBookings");
        }

        [HttpGet("Rooms")]
        public IActionResult Rooms()
        {
            // Để xem giao diện mà không cần dữ liệu từ DB
            var viewModel = new RoomsViewModel
            {
                AvailableCount = 32,
                OccupiedCount = 18,
                MaintenanceCount = 2,
                CleaningCount = 5,
                TotalRooms = 57,
                Rooms = GetSampleRooms()
            };

            return View("AdminRooms", viewModel);
        }

        private List<RoomItemViewModel> GetSampleRooms()
        {
            return new List<RoomItemViewModel>
    {
        new RoomItemViewModel
        {
            Id = 1,
            Name = "Phòng Deluxe Hướng Biển",
            RoomType = "Deluxe",
            PricePerNight = 2000000,
            Capacity = 2,
            Status = "Trống",
            Floor = 3,
            RoomNumber = "301",
            Rating = 4.8,
            ImageUrl = "/images/rooms/deluxe-ocean.jpg",
            Description = "Phòng sang trọng với tầm nhìn ra biển tuyệt đẹp",
            Amenities = new List<string> { "Wi-Fi", "TV", "Điều hòa", "Minibar", "Két an toàn" }
        },
        // Thêm các phòng mẫu khác tương tự
    };
        }

        [HttpGet("Users")]
        public IActionResult Users()
        {
            // Trả về giao diện quản lý khách hàng với dữ liệu mẫu
            var viewModel = new UsersViewModel
            {
                ActiveCount = 458,
                NewCount = 48,
                VipCount = 23,
                InactiveCount = 7,
                TotalUsers = 536,
                Users = GetSampleUsers()
            };

            return View("AdminUsers", viewModel);
        }

        private List<UserItemViewModel> GetSampleUsers()
        {
            return new List<UserItemViewModel>
    {
        new UserItemViewModel
        {
            Id = "1",
            UserId = "#UID001",
            FirstName = "Phạm",
            LastName = "Tuấn",
            Email = "phamtuan@mail.com",
            Phone = "0912345678",
            RegisterDate = DateTime.ParseExact("10/05/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 8,
            TotalSpending = 64500000,
            Status = "active",
            UserType = "vip",
            Address = "123 Phố Hàng Bài, Hoàn Kiếm, Hà Nội",
            LoyaltyPoints = 1450,
            LastLoginDate = DateTime.Now.AddHours(-2)
        },
        new UserItemViewModel
        {
            Id = "2",
            UserId = "#UID002",
            FirstName = "Nguyễn",
            LastName = "Hương",
            Email = "huongnguyen@mail.com",
            Phone = "0976543210",
            RegisterDate = DateTime.ParseExact("15/06/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 6,
            TotalSpending = 52800000,
            Status = "active",
            UserType = "vip",
            Address = "456 Phố Tràng Tiền, Hoàn Kiếm, Hà Nội",
            LoyaltyPoints = 1280,
            LastLoginDate = DateTime.Now.AddDays(-1)
        },
        new UserItemViewModel
        {
            Id = "3",
            UserId = "#UID003",
            FirstName = "Trần",
            LastName = "Linh",
            Email = "tranlinh@mail.com",
            Phone = "0932145678",
            RegisterDate = DateTime.ParseExact("22/07/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 5,
            TotalSpending = 48200000,
            Status = "active",
            UserType = "vip",
            Address = "789 Phố Bà Triệu, Hai Bà Trưng, Hà Nội",
            LoyaltyPoints = 950,
            LastLoginDate = DateTime.Now.AddDays(-3)
        },
        new UserItemViewModel
        {
            Id = "4",
            UserId = "#UID004",
            FirstName = "Lê",
            LastName = "Hoàng",
            Email = "lehoang@mail.com",
            Phone = "0987654321",
            RegisterDate = DateTime.ParseExact("05/09/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 2,
            TotalSpending = 12500000,
            Status = "inactive",
            UserType = "normal",
            Address = "101 Phố Láng Hạ, Đống Đa, Hà Nội",
            LoyaltyPoints = 250,
            LastLoginDate = DateTime.Now.AddDays(-10)
        },
        new UserItemViewModel
        {
            Id = "5",
            UserId = "#UID005",
            FirstName = "Vũ",
            LastName = "Minh",
            Email = "vuminh@mail.com",
            Phone = "0901234567",
            RegisterDate = DateTime.ParseExact("18/10/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 1,
            TotalSpending = 3200000,
            Status = "pending",
            UserType = "normal",
            Address = "202 Phố Hoàn Kiếm, Hoàn Kiếm, Hà Nội",
            LoyaltyPoints = 80,
            LastLoginDate = DateTime.Now.AddDays(-5)
        }
    };
        }

        [HttpGet("Reviews")]
        public IActionResult Reviews()
        {
            // Trả về giao diện quản lý đánh giá
            return View("AdminReviews");
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

        [HttpPost("AddRoom")]
        public async Task<IActionResult> AddRoom(RoomCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh (giả lập)
                    string mainImagePath = "/images/rooms/room-placeholder.jpg";
                    if (model.MainImage != null)
                    {
                        // Giả lập đường dẫn ảnh thành công
                        mainImagePath = $"/images/rooms/{Guid.NewGuid()}.jpg";
                    }

                    // Giả lập thành công và chuyển hướng
                    TempData["SuccessMessage"] = "Thêm phòng mới thành công!";
                    return RedirectToAction("Rooms");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi khi thêm phòng: {ex.Message}");
                }
            }

            return View(model);
        }

        [HttpPost("UpdateBookingStatus")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, int newStatusId, string reason = "Admin thay đổi trạng thái")
        {
            try
            {
                var bookingStatusService = HttpContext.RequestServices.GetService<IBookingStatusService>();
                if (bookingStatusService != null)
                {
                    await bookingStatusService.UpdateBookingStatusAsync(bookingId, newStatusId, reason);
                    TempData["SuccessMessage"] = "Cập nhật trạng thái đặt phòng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tìm thấy service để cập nhật trạng thái.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi khi cập nhật trạng thái: {ex.Message}";
            }

            return RedirectToAction("Bookings");
        }

        [HttpPost("UpdatePaymentStatus")]
        public async Task<IActionResult> UpdatePaymentStatus(int bookingId, int newPaymentStatusId, string reason = "Admin thay đổi trạng thái thanh toán")
        {
            try
            {
                var bookingStatusService = HttpContext.RequestServices.GetService<IBookingStatusService>();
                if (bookingStatusService != null)
                {
                    await bookingStatusService.UpdatePaymentStatusAsync(bookingId, newPaymentStatusId, reason);
                    TempData["SuccessMessage"] = "Cập nhật trạng thái thanh toán thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tìm thấy service để cập nhật trạng thái thanh toán.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi khi cập nhật trạng thái thanh toán: {ex.Message}";
            }

            return RedirectToAction("Bookings");
        }

        [HttpGet("EmailTest")]
        public IActionResult EmailTest()
        {
            return View();
        }
    }
}
