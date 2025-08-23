using HotelBookingSystem.Data;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public interface IAdminBookingService
    {
        Task<BookingsViewModel> GetBookings(BookingQueryOptions query);
        Task<(bool success, string message)> UpdateBookingStatus(int bookingId, string newStatus);
        Task<bool> UpdatePaymentStatus(int bookingId, string newPaymentStatus);
        Task<AdminBookingDetailsViewModel?> GetBookingDetails(int bookingId);
    }

    public class AdminBookingService : IAdminBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AdminBookingService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<BookingsViewModel> GetBookings(BookingQueryOptions query)
        {
            var queryable = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .AsQueryable();

            if (query.BookingId.HasValue)
                queryable = queryable.Where(b => b.Id == query.BookingId.Value);

            if (!string.IsNullOrEmpty(query.CustomerName))
                queryable = queryable.Where(b => b.User!.FullName!.Contains(query.CustomerName));

            if (!string.IsNullOrEmpty(query.Status))
                queryable = queryable.Where(b => b.BookingStatus.Name == query.Status);

            if (query.StartDate.HasValue)
                queryable = queryable.Where(b => b.CheckIn >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(b => b.CheckOut <= query.EndDate.Value);

            // Total count before paging
            var totalBookings = await queryable.CountAsync();

            // Paging
            var bookings = await queryable
                .OrderByDescending(b => b.CreatedDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BookingListItemViewModel
                {
                    Id = b.Id,
                    CustomerName = b.User.FullName ?? "",
                    CustomerEmail = b.User.Email ?? "",
                    RoomName = b.Room.Name,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Guests = b.Guests,
                    TotalPrice = b.TotalPrice,
                    BookingStatus = b.BookingStatus.Name,
                    PaymentStatus = b.Payment != null ? b.Payment.PaymentStatus.Name : "Đang xử lý"
                })
                .ToListAsync();

            var pending = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Chờ xác nhận");
            var confirmed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Đã xác nhận");
            var completed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Hoàn thành");
            var cancelled = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Đã hủy");

            return new BookingsViewModel
            {
                Bookings = bookings,
                CurrentPage = query.Page,
                TotalPages = (int)Math.Ceiling(totalBookings / (double)query.PageSize),
                TotalBookings = totalBookings,
                Query = query,
                Pending = pending,
                Confirmed = confirmed,
                Completed = completed,
                Cancelled = cancelled
            };
        }

        public async Task<(bool success, string message)> UpdateBookingStatus(int bookingId, string newStatus)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.Room)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy đặt phòng.");

            // Prevent invalid updates (already completed or cancelled)
            if (booking.BookingStatus.Name == "Hoàn thành" || booking.BookingStatus.Name == "Đã hủy")
                return (false, "Không thể cập nhật đặt phòng đã hoàn thành hoặc đã hủy.");

            // Kiểm tra payment status nếu muốn hoàn thành booking
            if (newStatus == "Hoàn thành")
            {
                if (booking.Payment == null || booking.Payment.PaymentStatus.Name != "Thành công")
                {
                    return (false, "Không thể hoàn thành đặt phòng. Trạng thái thanh toán phải là 'Thành công' trước khi hoàn thành đặt phòng.");
                }
            }

            // Xử lý hủy phòng và hoàn tiền
            if (newStatus == "Đã hủy")
            {
                // Nếu đã thanh toán thành công, tự động hoàn tiền
                if (booking.Payment != null && booking.Payment.PaymentStatus.Name == "Thành công")
                {
                    var refundStatus = await _context.PaymentStatuses.FirstOrDefaultAsync(s => s.Name == "Đã hoàn tiền");
                    if (refundStatus != null)
                    {
                        booking.Payment.PaymentStatus = refundStatus;
                        _context.Update(booking.Payment);
                    }
                }
            }

            // Set new status
            var newBookingStatus = await _context.BookingStatuses.FirstOrDefaultAsync(s => s.Name == newStatus);
            if (newBookingStatus != null)
            {
                booking.BookingStatus = newBookingStatus;

                // Cập nhật ngày hoàn thành khi trạng thái chuyển thành "Hoàn thành"
                if (newStatus == "Hoàn thành")
                {
                    booking.CompletedDate = DateTime.Now;
                }

                // Cập nhật trạng thái phòng dựa trên trạng thái booking
                if (booking.Room != null)
                {
                    if (newStatus == "Hoàn thành" || newStatus == "Đã hủy")
                    {
                        // Phòng trở lại khả dụng khi booking hoàn thành hoặc bị hủy
                        booking.Room.IsAvailable = true;
                    }
                    else if (newStatus == "Đã xác nhận")
                    {
                        // Đảm bảo phòng không khả dụng khi booking được xác nhận
                        booking.Room.IsAvailable = false;
                    }
                    _context.Rooms.Update(booking.Room);
                }
            }

            if (booking.BookingStatus == null)
                return (false, "Không tìm thấy trạng thái mới.");

            _context.Update(booking);
            await _context.SaveChangesAsync();

            string successMessage = newStatus switch
            {
                "Đã hủy" => booking.Payment?.PaymentStatus?.Name == "Đã hoàn tiền"
                    ? "Đã hủy đặt phòng và hoàn tiền thành công."
                    : "Đã hủy đặt phòng thành công.",
                "Hoàn thành" => "Đặt phòng đã được hoàn thành thành công.",
                "Đã xác nhận" => "Đã xác nhận đặt phòng thành công.",
                _ => "Cập nhật trạng thái thành công."
            };

            return (true, successMessage);
        }

        public async Task<bool> UpdatePaymentStatus(int bookingId, string newPaymentStatus)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Payment == null) return false;

            // Tìm payment status mới
            var newStatus = await _context.PaymentStatuses.FirstOrDefaultAsync(s => s.Name == newPaymentStatus);
            if (newStatus == null) return false;

            // Cập nhật payment status
            booking.Payment.PaymentStatus = newStatus;
            // booking.Payment.UpdatedDate = DateTime.Now; // Remove this line if UpdatedDate doesn't exist

            _context.Update(booking.Payment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AdminBookingDetailsViewModel?> GetBookingDetails(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return null;

            return new AdminBookingDetailsViewModel
            {
                // Booking Information
                Id = booking.Id,
                BookingNumber = $"#BK{booking.Id.ToString().PadLeft(6, '0')}",
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                Guests = booking.Guests,
                TotalPrice = booking.TotalPrice,
                CreatedDate = booking.CreatedDate,
                CompletedDate = booking.CompletedDate,
                Status = booking.BookingStatus.Name,
                SpecialRequests = "Không có yêu cầu đặc biệt", // Có thể thêm field này vào model sau

                // Customer Information
                CustomerName = booking.User.FullName ?? $"{booking.User.FirstName} {booking.User.LastName}".Trim(),
                CustomerEmail = booking.User.Email ?? "",
                CustomerPhone = booking.User.PhoneNumber ?? "Chưa cập nhật",
                CustomerJoinDate = DateTime.Now, // Sử dụng giá trị mặc định vì không có CreatedDate trong ApplicationUser

                // Room Information
                RoomId = booking.Room.Id,
                RoomName = booking.Room.Name,
                RoomType = booking.Room.RoomType,
                RoomDescription = booking.Room.Description ?? "Không có mô tả",
                RoomImageUrl = booking.Room.ImageUrl,
                RoomPricePerNight = booking.Room.PricePerNight,
                RoomCapacity = booking.Room.Capacity,
                RoomBedType = "Giường đôi", // Giá trị mặc định vì không có BedType trong Room model
                RoomFloor = "Tầng 1", // Giá trị mặc định vì không có Floor trong Room model
                RoomBuilding = "Tòa nhà chính", // Giá trị mặc định vì không có Building trong Room model

                // Payment Information
                PaymentId = booking.Payment?.Id,
                PaymentMethod = booking.Payment?.PaymentMethod ?? "Không xác định",
                PaymentStatus = booking.Payment?.PaymentStatus?.Name ?? "Đang xử lý",
                TransactionId = booking.Payment?.TransactionId ?? "",
                PaymentDate = booking.Payment?.PaymentDate,
                PaymentAmount = booking.Payment?.Amount ?? booking.TotalPrice,

                // Review Information
                HasReview = booking.Review != null,
                ReviewRating = booking.Review?.Rating,
                ReviewComment = booking.Review?.Comment ?? "",
                ReviewDate = booking.Review?.CreatedDate
            };
        }
    }
}
