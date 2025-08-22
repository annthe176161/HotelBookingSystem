using HotelBookingSystem.Data;
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

        public AdminBookingService(ApplicationDbContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return false;

            // Prevent invalid updates (already completed or cancelled)
            if (booking.BookingStatus.Name == "Hoàn thành" || booking.BookingStatus.Name == "Đã hủy")
                return false;

            // Set new status
            var newBookingStatus = await _context.BookingStatuses.FirstOrDefaultAsync(s => s.Name == newStatus);
            if (newBookingStatus != null)
                booking.BookingStatus = newBookingStatus;

            if (booking.BookingStatus == null) return false;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
