using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
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

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int roomId, DateTime? checkin, DateTime? checkout, int guests = 1)
        {
            var model = new BookingViewModel
            {
                RoomId = 1,
                RoomName = "Phòng Deluxe Hướng Biển",
                RoomType = "Deluxe",
                RoomImageUrl = "/images/rooms/deluxe-ocean.jpg",
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(2),
                GuestCount = 2,
                MaxGuests = 4,
                RoomPrice = 2000000,
                Discount = 0,
                FirstName = "Nguyen",
                LastName = "Van A",
                Email = "nguyenvana@example.com",
                Phone = "0123456789"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pendingStatus = await _context.BookingStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Chờ xác nhận");

                if (pendingStatus == null)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: Không tìm thấy trạng thái đặt phòng.");
                    return View(model);
                }

                var booking = new Booking
                {
                    RoomId = model.RoomId,
                    CheckIn = model.CheckInDate,
                    CheckOut = model.CheckOutDate,
                    Guests = model.GuestCount,
                    TotalPrice = model.TotalPrice,
                    CreatedDate = DateTime.Now,
                    BookingStatusId = pendingStatus.Id,
                };

                if (User.Identity.IsAuthenticated)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                    if (user != null)
                    {
                        booking.UserId = user.Id;
                    }
                }
                else
                {
                    // TODO: Xử lý đặt phòng cho khách không đăng nhập
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction("Confirmation", new { id = booking.Id });
            }

            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room != null)
            {
                model.RoomName = room.Name;
                model.RoomType = room.RoomType;
                model.RoomImageUrl = room.ImageUrl;
                model.MaxGuests = room.Capacity;
                model.RoomPrice = room.PricePerNight;
            }

            return View(model);
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public async Task<IActionResult> Index(string status = null)
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
