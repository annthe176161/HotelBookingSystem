using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Account;
using HotelBookingSystem.ViewModels.Booking;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(ApplicationDbContext context, IBookingService bookingService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _bookingService = bookingService;
            _userManager = userManager;
        }

        // Action để hiển thị danh sách booking của khách hàng
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", string status = "")
        {
            // Lấy user mặc định để test (có thể thay đổi khi có login)
            var user = await _userManager.FindByEmailAsync("test.user@example.com");
            if (user == null)
            {
                TempData["Error"] = "Tài khoản test mặc định chưa được tạo. Vui lòng chạy lại ứng dụng để seed dữ liệu.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = await _bookingService.GetCustomerBookingsAsync(user.Id, searchTerm, status);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int roomId, DateTime? checkin, DateTime? checkout, int guests = 1)
        {
            // Lấy thông tin phòng từ database
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                TempData["Error"] = "Phòng không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // --- BẮT ĐẦU THAY ĐỔI: Lấy user mặc định để test ---
            var user = await _userManager.FindByEmailAsync("test.user@example.com");
            if (user == null)
            {
                // Xử lý trường hợp không tìm thấy user test. 
                // Có thể tạo user ở đây hoặc báo lỗi.
                // Trong ví dụ này, chúng ta sẽ báo lỗi để đảm bảo SeedData đã chạy đúng.
                TempData["Error"] = "Tài khoản test mặc định chưa được tạo. Vui lòng chạy lại ứng dụng để seed dữ liệu.";
                return RedirectToAction("Index", "Home");
            }
            // --- KẾT THÚC THAY ĐỔI ---

            // Thiết lập ngày mặc định nếu không có
            var checkInDate = checkin ?? DateTime.Today.AddDays(1);
            var checkOutDate = checkout ?? DateTime.Today.AddDays(2);

            // Đảm bảo ngày checkout sau ngày checkin
            if (checkOutDate <= checkInDate)
            {
                checkOutDate = checkInDate.AddDays(1);
            }

            var model = new BookingViewModel
            {
                RoomId = roomId,
                RoomName = room.Name,
                RoomType = room.RoomType,
                RoomImageUrl = room.ImageUrl,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                GuestCount = Math.Min(guests, room.Capacity),
                MaxGuests = room.Capacity,
                RoomPrice = room.PricePerNight
            };

            // Điền thông tin từ user test vào model
            model.Email = user.Email;
            model.Phone = user.PhoneNumber;
            if (!string.IsNullOrEmpty(user.FullName))
            {
                var nameParts = user.FullName.Split(' ', 2);
                model.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                model.LastName = nameParts.Length > 1 ? nameParts[1] : "";
            }


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            // Debug: Log ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                     .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                
                foreach (var error in errors)
                {
                    foreach (var errorMsg in error.Errors)
                    {
                        // Thêm lỗi vào ModelState để hiển thị
                        ModelState.AddModelError("", $"{error.Field}: {errorMsg}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validation
                    if (model.CheckOutDate <= model.CheckInDate)
                    {
                        ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải sau ngày nhận phòng.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }

                    if (model.CheckInDate < DateTime.Today)
                    {
                        ModelState.AddModelError("CheckInDate", "Ngày nhận phòng không thể là ngày trong quá khứ.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }

                    // Kiểm tra tính khả dụng của phòng
                    var isAvailable = await _bookingService.IsRoomAvailableAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);
                    if (!isAvailable)
                    {
                        ModelState.AddModelError("", "Phòng không khả dụng trong thời gian bạn đã chọn. Vui lòng chọn ngày khác.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }

                    // --- BẮT ĐẦU THAY ĐỔI: Lấy userId của user mặc định ---
                    var user = await _userManager.FindByEmailAsync("test.user@example.com");
                    var userId = user?.Id;

                    if (userId == null)
                    {
                        // Trường hợp này không nên xảy ra nếu GET hoạt động đúng
                        ModelState.AddModelError("", "Không thể xác định người dùng để đặt phòng.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }
                    // --- KẾT THÚC THAY ĐỔI ---

                    // Tạo booking
                    var booking = await _bookingService.CreateBookingAsync(model, userId);

                    TempData["Success"] = "Đặt phòng thành công! Chúng tôi sẽ liên hệ với bạn sớm nhất.";
                    return RedirectToAction("Confirmation", new { id = booking.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", $"Chi tiết: {ex.InnerException.Message}");
                    }
                }
            }

            // Nếu có lỗi, load lại thông tin phòng
            await RepopulateRoomInfoForModel(model);
            return View(model);
        }

        private async Task RepopulateRoomInfoForModel(BookingViewModel model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room != null)
            {
                model.RoomName = room.Name;
                model.RoomType = room.RoomType;
                model.RoomImageUrl = room.ImageUrl;
                model.MaxGuests = room.Capacity;
                model.RoomPrice = room.PricePerNight;
            }
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đặt phòng.";
                return RedirectToAction("Index", "Home");
            }

            return View(booking);
        }

        public async Task<IActionResult> Index(string? status = null)
        {
            var viewModel = new BookingListViewModel
            {
                Status = status,
                Bookings = GetSampleBookings()
            };

            return View(viewModel);
        }

        // Explicitly use HotelBookingSystem.ViewModels.Booking.BookingItemViewModel to resolve ambiguity
        private List<HotelBookingSystem.ViewModels.Booking.BookingItemViewModel> GetSampleBookings()
        {
            return new List<HotelBookingSystem.ViewModels.Booking.BookingItemViewModel>
                {
                    new HotelBookingSystem.ViewModels.Booking.BookingItemViewModel
                    {
                        Id = 1,
                        BookingNumber = "B001-2023",
                        RoomName = "Phòng Deluxe Hướng Biển",
                        RoomImageUrl = "/images/rooms/deluxe-ocean.jpg",
                        CheckInDate = DateTime.Now.AddDays(7),
                        CheckOutDate = DateTime.Now.AddDays(10),
                        GuestsCount = 2,
                        TotalPrice = 6000000,
                        Status = "Chờ xác nhận",
                        BookingDate = DateTime.Now.AddDays(-2),
                        HasReview = false
                    },
                    new HotelBookingSystem.ViewModels.Booking.BookingItemViewModel
                    {
                        Id = 2,
                        BookingNumber = "B002-2023",
                        RoomName = "Phòng Suite Gia Đình",
                        RoomImageUrl = "/images/rooms/family-suite.jpg",
                        CheckInDate = DateTime.Now.AddDays(-15),
                        CheckOutDate = DateTime.Now.AddDays(-10),
                        GuestsCount = 4,
                        TotalPrice = 17500000,
                        Status = "Hoàn thành",
                        BookingDate = DateTime.Now.AddDays(-20),
                        HasReview = true
                    },
                    new HotelBookingSystem.ViewModels.Booking.BookingItemViewModel
                    {
                        Id = 3,
                        BookingNumber = "B003-2023",
                        RoomName = "Phòng Standard Giường Đôi",
                        RoomImageUrl = "/images/rooms/standard-twin.jpg",
                        CheckInDate = DateTime.Now.AddDays(-5),
                        CheckOutDate = DateTime.Now.AddDays(-2),
                        GuestsCount = 2,
                        TotalPrice = 3600000,
                        Status = "Đã xác nhận",
                        BookingDate = DateTime.Now.AddDays(-7),
                        HasReview = false
                    },
                    new HotelBookingSystem.ViewModels.Booking.BookingItemViewModel
                    {
                        Id = 4,
                        BookingNumber = "B004-2023",
                        RoomName = "Phòng Executive Business",
                        RoomImageUrl = "/images/rooms/executive-business.jpg",
                        CheckInDate = DateTime.Now.AddDays(3),
                        CheckOutDate = DateTime.Now.AddDays(5),
                        GuestsCount = 1,
                        TotalPrice = 3600000,
                        Status = "Đã hủy",
                        BookingDate = DateTime.Now.AddDays(-10),
                        HasReview = false
                    },
                    new HotelBookingSystem.ViewModels.Booking.BookingItemViewModel
                    {
                        Id = 5,
                        BookingNumber = "B005-2023",
                        RoomName = "Phòng Tổng Thống",
                        RoomImageUrl = "/images/rooms/presidential-suite.jpg",
                        CheckInDate = DateTime.Now.AddDays(14),
                        CheckOutDate = DateTime.Now.AddDays(16),
                        GuestsCount = 2,
                        TotalPrice = 16000000,
                        Status = "Đã xác nhận",
                        BookingDate = DateTime.Now.AddDays(-1),
                        HasReview = false
                    }
                };
        }

        public IActionResult Details(int id)
        {
            // Cho mục đích demo, tạo dữ liệu mẫu
            var viewModel = new BookingDetailsViewModel
            {
                Id = id,
                BookingNumber = $"B{id:D3}-2023",
                Status = "Đã xác nhận",
                BookingDate = DateTime.Now.AddDays(-5),

                // Thông tin phòng
                RoomId = 1,
                RoomName = "Phòng Deluxe Hướng Biển",
                RoomType = "Deluxe",
                RoomDescription = "Phòng sang trọng với tầm nhìn tuyệt đẹp ra biển và các tiện nghi hiện đại.",
                RoomImageUrl = "/images/rooms/deluxe-ocean.jpg",
                RoomRating = 4.8,
                RoomCapacity = 2,
                RoomSize = 45,
                BedType = "1 Giường King",
                Floor = "3",
                Building = "Tòa chính",

                // Thông tin lưu trú
                CheckInDate = DateTime.Now.AddDays(3),
                CheckOutDate = DateTime.Now.AddDays(6),
                GuestsCount = 2,
                SpecialRequests = "Tôi muốn phòng trên tầng cao và xa thang máy.",

                // Thông tin khách hàng
                GuestName = "Nguyễn Văn A",
                GuestEmail = "nguyenvana@example.com",
                GuestPhone = "0912345678",

                // Thông tin thanh toán
                RoomPrice = 6000000,
                ServiceFee = 600000,
                TaxFee = 480000,
                Discount = 0,
                TotalPrice = 7080000,
                PaymentMethod = "Thanh toán tại khách sạn",
                PaymentStatus = "Chờ thanh toán",
                PaymentDetails = null,

                // Chính sách hủy phòng
                IsCancellable = true,
                FreeCancellationDeadline = DateTime.Now.AddDays(1),

                // Đánh giá
                CanReview = false,
                HasReview = false,

                // Lịch sử hoạt động
                BookingActivities = new List<BookingActivityViewModel>
        {
            new BookingActivityViewModel
            {
                Date = DateTime.Now.AddDays(-5),
                Title = "Đã tạo đặt phòng",
                Description = "Đơn đặt phòng đã được tạo thành công.",
                Type = "create"
            },
            new BookingActivityViewModel
            {
                Date = DateTime.Now.AddDays(-4),
                Title = "Đã xác nhận đặt phòng",
                Description = "Đơn đặt phòng đã được xác nhận.",
                Type = "confirm"
            },
            new BookingActivityViewModel
            {
                Date = DateTime.Now.AddDays(-3),
                Title = "Cập nhật yêu cầu đặc biệt",
                Description = "Thêm yêu cầu đặc biệt về vị trí phòng.",
                Type = "update"
            }
        }
            };

            return View(viewModel);
        }
    }
}
