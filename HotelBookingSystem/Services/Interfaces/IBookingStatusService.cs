using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services.Interfaces
{
    public interface IBookingStatusService
    {
        Task UpdateBookingStatusAsync(int bookingId, int newStatusId, string reason = "");
        Task UpdatePaymentStatusAsync(int bookingId, int newPaymentStatusId, string reason = "");
        Task CancelBookingAsync(int bookingId, string reason);
        Task SendCheckInReminderAsync(int bookingId);
        Task SendPaymentReminderAsync(int bookingId);
    }
}
