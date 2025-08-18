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

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
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

            // Tính toán tổng giá
            var totalPrice = await CalculateTotalPriceAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);

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

            return booking;
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
    }
}
