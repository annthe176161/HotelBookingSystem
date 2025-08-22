using HotelBookingSystem.Data;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public interface IAdminBookingService
    {
        Task<BookingsViewModel> GetBookings(BookingQueryOptions options);
        Task<bool> UpdateBookingStatus(int bookingId, string newStatus);
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

        public async Task<BookingsViewModel> GetBookings(BookingQueryOptions options)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                    .ThenInclude(p => p.PaymentStatus)
                .AsQueryable();

            if (options.BookingId.HasValue)
                query = query.Where(b => b.Id == options.BookingId.Value);

            if (!string.IsNullOrEmpty(options.CustomerName))
                query = query.Where(b => b.User.FullName.Contains(options.CustomerName));

            if (!string.IsNullOrEmpty(options.Status))
                query = query.Where(b => b.BookingStatus.Name == options.Status);

            if (options.StartDate.HasValue)
                query = query.Where(b => b.CheckIn >= options.StartDate.Value);

            if (options.EndDate.HasValue)
                query = query.Where(b => b.CheckOut <= options.EndDate.Value);

            // Total count before paging
            var totalBookings = await query.CountAsync();

            // Paging
            var bookings = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip((options.Page - 1) * options.PageSize)
                .Take(options.PageSize)
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
                    PaymentStatus = b.Payment != null ? b.Payment.PaymentStatus.Name : "Thanh toán đang được xử lý"
                })
                .ToListAsync();

            var pending = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Chờ xác nhận");
            var confirmed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Đã xác nhận");
            var completed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Hoàn thành");
            var cancelled = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Đã hủy");

            return new BookingsViewModel
            {
                Bookings = bookings,
                CurrentPage = options.Page,
                TotalPages = (int)Math.Ceiling(totalBookings / (double)options.PageSize),
                TotalBookings = totalBookings,
                Query = options,
                Pending = pending,
                Confirmed = confirmed,
                Completed = completed,
                Cancelled = cancelled
            };
        }

        public async Task<bool> UpdateBookingStatus(int bookingId, string newStatus)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return false;

            // Prevent invalid updates (already completed or cancelled)
            if (booking.BookingStatus.Name == "Hoàn thành" || booking.BookingStatus.Name == "Đã hủy")
                return false;

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

            if (booking.BookingStatus == null) return false;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
