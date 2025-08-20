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
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(ApplicationDbContext context, IBookingService bookingService, IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _bookingService = bookingService;
            _emailService = emailService;
            _userManager = userManager;
        }

        // Action để hiển thị danh sách booking của khách hàng
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string roomType = "", string paymentStatus = "")
        {
            // Lấy user mặc định để test (có thể thay đổi khi có login)
            var user = await _userManager.FindByEmailAsync("thanhan01236339441@gmail.com");
            if (user == null)
            {
                TempData["Error"] = "Tài khoản test mặc định chưa được tạo. Vui lòng chạy lại ứng dụng để seed dữ liệu.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy danh sách status từ DB để hiển thị trong dropdown
            var bookingStatuses = await _context.BookingStatuses
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var paymentStatuses = await _context.PaymentStatuses
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var roomTypes = await _context.Rooms
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(rt => rt)
                .ToListAsync();

            ViewBag.BookingStatuses = bookingStatuses;
            ViewBag.PaymentStatuses = paymentStatuses;
            ViewBag.RoomTypes = roomTypes;

            var viewModel = await _bookingService.GetCustomerBookingsAsync(user.Id, searchTerm, status, roomType, paymentStatus);
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
            var user = await _userManager.FindByEmailAsync("thanhan01236339441@gmail.com");
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
            // Debug: Log tất cả dữ liệu nhận được
            Console.WriteLine($"=== DEBUG CREATE BOOKING ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"RoomId: {model.RoomId}");
            Console.WriteLine($"CheckIn: {model.CheckInDate}");
            Console.WriteLine($"CheckOut: {model.CheckOutDate}");
            Console.WriteLine($"Guests: {model.GuestCount}");
            Console.WriteLine($"FirstName: {model.FirstName}");
            Console.WriteLine($"LastName: {model.LastName}");
            Console.WriteLine($"Email: {model.Email}");
            Console.WriteLine($"Phone: {model.Phone}");

            // Debug: Log ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ModelState Errors ===");
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                     .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Field}");
                    foreach (var errorMsg in error.Errors)
                    {
                        Console.WriteLine($"  Error: {errorMsg}");
                        // Thêm lỗi vào ModelState để hiển thị
                        ModelState.AddModelError("", $"{error.Field}: {errorMsg}");
                    }
                }
                Console.WriteLine("=== End ModelState Errors ===");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("ModelState is valid, starting booking process...");

                    // Validation
                    if (model.CheckOutDate <= model.CheckInDate)
                    {
                        Console.WriteLine("Error: CheckOut date is not after CheckIn date");
                        ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải sau ngày nhận phòng.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }

                    if (model.CheckInDate < DateTime.Today)
                    {
                        Console.WriteLine("Error: CheckIn date is in the past");
                        ModelState.AddModelError("CheckInDate", "Ngày nhận phòng không thể là ngày trong quá khứ.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }

                    // Kiểm tra tính khả dụng của phòng
                    Console.WriteLine("Checking room availability...");
                    var isAvailable = await _bookingService.IsRoomAvailableAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);
                    if (!isAvailable)
                    {
                        Console.WriteLine("Error: Room is not available");
                        ModelState.AddModelError("", "Phòng không khả dụng trong thời gian bạn đã chọn. Vui lòng chọn ngày khác.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }

                    // --- BẮT ĐẦU THAY ĐỔI: Lấy userId của user mặc định ---
                    Console.WriteLine("Finding test user...");
                    var user = await _userManager.FindByEmailAsync("thanhan01236339441@gmail.com");
                    var userId = user?.Id;

                    if (userId == null)
                    {
                        Console.WriteLine("Error: Cannot find test user");
                        // Trường hợp này không nên xảy ra nếu GET hoạt động đúng
                        ModelState.AddModelError("", "Không thể xác định người dùng để đặt phòng.");
                        await RepopulateRoomInfoForModel(model);
                        return View(model);
                    }
                    Console.WriteLine($"Found user: {userId}");
                    // --- KẾT THÚC THAY ĐỔI ---

                    // Tạo booking
                    Console.WriteLine("Creating booking...");
                    var booking = await _bookingService.CreateBookingAsync(model, userId);
                    Console.WriteLine($"Booking created with ID: {booking.Id}");

                    // Gửi email thông báo
                    try
                    {
                        Console.WriteLine("Sending emails...");
                        // Gửi email xác nhận cho khách hàng
                        await _emailService.SendBookingConfirmationToCustomerAsync(booking);
                        Console.WriteLine("Customer email sent successfully");

                        // Gửi email thông báo cho khách sạn
                        await _emailService.SendBookingNotificationToHotelAsync(booking);
                        Console.WriteLine("Hotel email sent successfully");

                        TempData["Success"] = "Đặt phòng thành công! Email xác nhận đã được gửi đến địa chỉ email của bạn.";
                    }
                    catch (Exception emailEx)
                    {
                        // Log lỗi email nhưng không làm fail transaction
                        Console.WriteLine($"Email sending failed: {emailEx.Message}");
                        TempData["Success"] = "Đặt phòng thành công! Tuy nhiên, có lỗi khi gửi email xác nhận. Chúng tôi sẽ liên hệ với bạn sớm nhất.";
                    }

                    Console.WriteLine($"Redirecting to Confirmation with booking ID: {booking.Id}");
                    return RedirectToAction("Confirmation", new { id = booking.Id });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in Create method: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                        ModelState.AddModelError("", $"Chi tiết: {ex.InnerException.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("ModelState is NOT valid, returning to view");
            }

            // Nếu có lỗi, load lại thông tin phòng
            Console.WriteLine("Repopulating room info and returning view");
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

        public async Task<IActionResult> Details(int id)
        {
            // Lấy user mặc định để test
            var user = await _userManager.FindByEmailAsync("thanhan01236339441@gmail.com");
            if (user == null)
            {
                TempData["Error"] = "Tài khoản test mặc định chưa được tạo.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = await _bookingService.GetBookingDetailsAsync(id, user.Id);
            if (viewModel == null)
            {
                TempData["Error"] = "Không tìm thấy đặt phòng hoặc bạn không có quyền xem.";
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            // Lấy user mặc định để test
            var user = await _userManager.FindByEmailAsync("thanhan01236339441@gmail.com");
            if (user == null)
            {
                TempData["Error"] = "Tài khoản test mặc định chưa được tạo.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _bookingService.CancelBookingAsync(id, user.Id);

            if (result)
            {
                TempData["Success"] = "Đặt phòng đã được hủy thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đặt phòng. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}
