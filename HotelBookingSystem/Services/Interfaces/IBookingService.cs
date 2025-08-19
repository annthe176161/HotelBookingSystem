using HotelBookingSystem.ViewModels.Booking;

namespace HotelBookingSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<decimal> CalculateTotalPriceAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<Models.Booking> CreateBookingAsync(ViewModels.Booking.BookingViewModel model, string? userId = null);
        Task<Models.Booking?> GetBookingByIdAsync(int bookingId);
        Task<List<Models.Booking>> GetUserBookingsAsync(string userId);
        Task<CustomerBookingsViewModel> GetCustomerBookingsAsync(string userId, string searchTerm = "", string status = "");
    }
}
