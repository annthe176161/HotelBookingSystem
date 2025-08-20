using HotelBookingSystem.Data;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public interface IAdminBookingService
    {
        Task<BookingsViewModel> GetBookings(BookingQueryOptions options);
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
                    CustomerName = b.User.FullName,
                    CustomerEmail = b.User.Email ?? "",
                    RoomName = b.Room.Name,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Guests = b.Guests,
                    TotalPrice = b.TotalPrice,
                    BookingStatus = b.BookingStatus.Name,
                    PaymentStatus = b.Payment != null ? b.Payment.PaymentStatus.Name : "Chưa thanh toán"
                })
                .ToListAsync();

            var pending = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Pending");
            var confirmed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Confirmed");
            var completed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Completed");
            var cancelled = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Cancelled");

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
    }
}
