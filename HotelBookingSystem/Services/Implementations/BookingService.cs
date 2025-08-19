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

        public async Task<CustomerBookingsViewModel> GetCustomerBookingsAsync(string userId, string searchTerm = "", string status = "")
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
                Bookings = customerBookings,
                TotalBookings = customerBookings.Count,
                CompletedBookings = customerBookings.Count(b => b.Status == "Completed"),
                CancelledBookings = customerBookings.Count(b => b.Status == "Cancelled"),
                PendingBookings = customerBookings.Count(b => b.Status == "Confirmed" || b.Status == "Pending")
            };

            return viewModel;
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Confirmed" => "success",
                "Pending" => "warning", 
                "Cancelled" => "danger",
                "Completed" => "primary",
                _ => "secondary"
            };
        }
    }
}
