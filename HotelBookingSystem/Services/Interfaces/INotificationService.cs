using HotelBookingSystem.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HotelBookingSystem.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendBookingNotificationToAdminAsync(string bookingId, string customerName, string roomName, string message);
        Task SendBookingConfirmationToCustomerAsync(string userId, string bookingId, string roomName, string message);
        Task SendBookingStatusUpdateToCustomerAsync(string userId, string bookingId, string status, string message);
        Task SendPaymentNotificationAsync(string userId, string bookingId, string paymentStatus, string message);
        Task SendGeneralNotificationToUserAsync(string userId, string message, string type = "info");
        Task SendGeneralNotificationToAdminsAsync(string message, string type = "info", object? data = null);
        Task SendBookingCancellationToAdminAsync(int bookingId, string customerName, string roomName, string reason);
        Task SendReviewNotificationToAdminAsync(int bookingId, string customerName, string roomName, int rating, string comment);
    }
}
