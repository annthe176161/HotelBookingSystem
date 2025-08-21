using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Booking;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public BookingService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var conflictingBookings = await _context.Bookings
                .Where(b => b.RoomId == roomId &&
                           b.BookingStatus.Name != "Đã hủy" &&
                           ((b.CheckIn < checkOut && b.CheckOut > checkIn)))
                .AnyAsync();

            return !conflictingBookings;
        }

        public async Task<decimal> CalculateTotalPriceAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return 0;

            var nights = (checkOut - checkIn).Days;
            return room.PricePerNight * nights;
        }

        public async Task<Models.Booking> CreateBookingAsync(BookingViewModel model, string? userId = null)
        {
            // Kiểm tra tính khả dụng của phòng
            var isAvailable = await IsRoomAvailableAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);
            if (!isAvailable)
            {
                throw new InvalidOperationException("Phòng không khả dụng trong thời gian đã chọn.");
            }

            // Lấy trạng thái đặt phòng mặc định
            var pendingStatus = await _context.BookingStatuses
                .FirstOrDefaultAsync(s => s.Name == "Chờ xác nhận");

            if (pendingStatus == null)
            {
                throw new InvalidOperationException("Không tìm thấy trạng thái đặt phòng.");
            }

            // Lấy trạng thái thanh toán mặc định (Đang xử lý)
            var pendingPaymentStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Đang xử lý");

            if (pendingPaymentStatus == null)
            {
                throw new InvalidOperationException("Không tìm thấy trạng thái thanh toán.");
            }

            // Tính toán tổng giá
            var totalPrice = await CalculateTotalPriceAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);

            // Sử dụng transaction để đảm bảo tính nhất quán dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo booking
                var booking = new Models.Booking
                {
                    RoomId = model.RoomId,
                    CheckIn = model.CheckInDate,
                    CheckOut = model.CheckOutDate,
                    Guests = model.GuestCount,
                    TotalPrice = totalPrice,
                    CreatedDate = DateTime.Now,
                    BookingStatusId = pendingStatus.Id,
                    UserId = userId
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Tạo payment record
                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = totalPrice,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = "Thanh toán tại khách sạn", // Có thể thay đổi theo yêu cầu
                    TransactionId = $"TXN{booking.Id:D8}{DateTime.Now:yyyyMMddHHmmss}", // Tạo transaction ID
                    PaymentStatusId = pendingPaymentStatus.Id
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Load lại booking với đầy đủ thông tin cho email
                var bookingWithDetails = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .Include(b => b.BookingStatus)
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.Id == booking.Id);

                return bookingWithDetails ?? booking;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Models.Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.User)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<List<Models.Booking>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<CustomerBookingsViewModel> GetCustomerBookingsAsync(string userId, string searchTerm = "", string status = "", string roomType = "", string paymentStatus = "")
        {
            var query = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                    .ThenInclude(p => p.PaymentStatus)
                .Where(b => b.UserId == userId);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b =>
                    b.Room.Name.Contains(searchTerm) ||
                    b.Room.RoomType.Contains(searchTerm) ||
                    b.Id.ToString().Contains(searchTerm));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.BookingStatus.Name == status);
            }

            // Apply room type filter
            if (!string.IsNullOrEmpty(roomType))
            {
                query = query.Where(b => b.Room.RoomType == roomType);
            }

            // Apply payment status filter
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(b => b.Payment != null && b.Payment.PaymentStatus.Name == paymentStatus);
            }

            var bookings = await query
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            var customerBookings = bookings.Select(b => new CustomerBookingItem
            {
                Id = b.Id,
                BookingNumber = $"#BK{b.Id.ToString().PadLeft(6, '0')}",
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Guests = b.Guests,
                TotalPrice = b.TotalPrice,
                CreatedDate = b.CreatedDate,
                CompletedDate = b.CompletedDate,

                // Room Information
                RoomId = b.RoomId,
                RoomName = b.Room.Name,
                RoomType = b.Room.RoomType,
                RoomImageUrl = b.Room.ImageUrl,
                RoomPricePerNight = b.Room.PricePerNight,

                // Status Information
                Status = b.BookingStatus.Name,
                StatusColor = GetStatusColor(b.BookingStatus.Name),

                // Payment Information
                PaymentStatus = b.Payment?.PaymentStatus?.Name ?? "Pending",
                PaymentMethod = b.Payment?.PaymentMethod ?? "N/A"
            }).ToList();

            var viewModel = new CustomerBookingsViewModel
            {
                SearchTerm = searchTerm,
                Status = status,
                RoomType = roomType,
                PaymentStatus = paymentStatus,
                Bookings = customerBookings,
                TotalBookings = customerBookings.Count,
                CompletedBookings = customerBookings.Count(b => b.Status == "Hoàn thành"),
                CancelledBookings = customerBookings.Count(b => b.Status == "Đã hủy"),
                PendingBookings = customerBookings.Count(b => b.Status == "Đã xác nhận" || b.Status == "Chờ xác nhận")
            };

            return viewModel;
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Đã xác nhận" => "success",
                "Chờ xác nhận" => "warning",
                "Đã hủy" => "danger",
                "Hoàn thành" => "primary",
                _ => "secondary"
            };
        }

        public async Task<BookingDetailsViewModel?> GetBookingDetailsAsync(int bookingId, string userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                .ThenInclude(p => p!.PaymentStatus)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
                return null;

            var nightCount = (booking.CheckOut - booking.CheckIn).Days;
            var canCancel = booking.BookingStatus.Name != "Hoàn thành" && booking.BookingStatus.Name != "Đã hủy";

            return new BookingDetailsViewModel
            {
                Id = booking.Id,
                BookingNumber = $"BK{booking.Id:D6}",
                Status = booking.BookingStatus.Name,
                BookingDate = booking.CreatedDate,

                // Thông tin phòng
                RoomId = booking.RoomId,
                RoomName = booking.Room.Name,
                RoomType = booking.Room.RoomType,
                RoomDescription = booking.Room.Description,
                RoomImageUrl = booking.Room.ImageUrl ?? "/images/rooms/default.jpg",
                RoomRating = booking.Room.AverageRating,
                RoomCapacity = booking.Room.Capacity,
                RoomSize = 45, // Có thể thêm field này vào Room model sau
                BedType = "1 Giường King", // Có thể thêm field này vào Room model sau
                Floor = "3", // Có thể thêm field này vào Room model sau
                Building = "Tòa chính", // Có thể thêm field này vào Room model sau

                // Thông tin lưu trú
                CheckInDate = booking.CheckIn,
                CheckOutDate = booking.CheckOut,
                GuestsCount = booking.Guests,
                SpecialRequests = "", // Có thể thêm field này vào Booking model sau

                // Thông tin khách hàng
                GuestName = booking.User.FullName ?? "N/A",
                GuestEmail = booking.User.Email ?? "N/A",
                GuestPhone = booking.User.PhoneNumber ?? "N/A",

                // Thông tin thanh toán
                RoomPrice = booking.Room.PricePerNight * nightCount,
                ServiceFee = 0, // Có thể thêm logic tính phí dịch vụ
                TaxFee = 0, // Có thể thêm logic tính thuế
                Discount = 0, // Có thể thêm logic giảm giá
                TotalPrice = booking.TotalPrice,
                PaymentMethod = booking.Payment?.PaymentMethod ?? "Thanh toán tại khách sạn",
                PaymentStatus = booking.Payment?.PaymentStatus?.Name ?? "Pending",
                PaymentDetails = null,

                // Chính sách hủy phòng
                IsCancellable = canCancel,
                FreeCancellationDeadline = booking.CheckIn.AddDays(-1), // Có thể hủy miễn phí trước 1 ngày

                // Đánh giá
                CanReview = booking.BookingStatus.Name == "Hoàn thành" && booking.CheckOut < DateTime.Now,
                HasReview = false, // Có thể thêm logic kiểm tra review

                // Lịch sử hoạt động
                BookingActivities = new List<BookingActivityViewModel>
                {
                    new BookingActivityViewModel
                    {
                        Date = booking.CreatedDate,
                        Title = "Đã tạo đặt phòng",
                        Description = "Đơn đặt phòng đã được tạo thành công.",
                        Type = "create"
                    }
                }
            };
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                .ThenInclude(p => p!.PaymentStatus)
                .Include(b => b.User)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
                return false;

            // Chỉ cho phép hủy nếu booking chưa hoàn thành
            if (booking.BookingStatus.Name == "Hoàn thành")
                return false;

            // Lưu trạng thái cũ để gửi email
            var oldStatus = booking.BookingStatus.Name;

            // Lấy trạng thái "Đã hủy"
            var cancelledStatus = await _context.BookingStatuses
                .FirstOrDefaultAsync(s => s.Name == "Đã hủy");

            if (cancelledStatus == null)
                return false;

            // Cập nhật trạng thái booking
            booking.BookingStatusId = cancelledStatus.Id;

            // Cập nhật trạng thái payment nếu có
            if (booking.Payment != null)
            {
                var cancelledPaymentStatus = await _context.PaymentStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Cancelled");

                if (cancelledPaymentStatus != null)
                {
                    booking.Payment.PaymentStatusId = cancelledPaymentStatus.Id;
                }
            }

            await _context.SaveChangesAsync();

            // Gửi email thông báo hủy đặt phòng
            try
            {
                string reason = "Khách hàng yêu cầu hủy đặt phòng";
                await _emailService.SendBookingCancellationToCustomerAsync(booking, reason);
                await _emailService.SendBookingCancellationToHotelAsync(booking, reason);
            }
            catch (Exception ex)
            {
                // Log lỗi email nhưng không làm fail transaction
                Console.WriteLine($"Email sending failed during booking cancellation: {ex.Message}");
            }

            return true;
        }

        public async Task<string> CreateBookingReviewAsync(BookingReviewViewModel request, string userId)
        {
            // Sửa: BookingId là int, không thể null, nên kiểm tra > 0
            if (request.BookingId <= 0)
            {
                return "Booking không hợp lệ";
            }

            var booking = await _context.Bookings.FindAsync(request.BookingId); // Sử dụng FindAsync
            if (booking == null)
            {
                return "Booking không tồn tại";
            }

            // Kiểm tra xem đã có review chưa
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == request.BookingId && r.UserId == userId);

            if (existingReview != null)
            {
                return "Bạn đã đánh giá rồi"; // Đã đánh giá rồi
            }

            var bookingReview = new Models.Review
            {
                BookingId = booking.Id,
                Rating = request.Rating,
                RoomId = booking.RoomId,
                Comment = request.Comment,
                UserId = userId,
                CreatedDate = DateTime.Now // Thêm thời gian tạo
            };

            _context.Reviews.Add(bookingReview);
            await _context.SaveChangesAsync();
            return "Đánh giá thành công";
        }

    }
}
