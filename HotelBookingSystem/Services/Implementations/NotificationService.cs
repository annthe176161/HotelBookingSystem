using HotelBookingSystem.Infrastructure.Hubs;
using HotelBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HotelBookingSystem.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendBookingNotificationToAdminAsync(string bookingId, string customerName, string roomName, string message)
        {
            var notification = new
            {
                message = message,
                type = "booking",
                timestamp = DateTime.Now,
                data = new
                {
                    bookingId,
                    customerName,
                    roomName,
                    action = "new_booking"
                }
            };

            await _hubContext.Clients.Group("AdminGroup").SendAsync("ReceiveAdminNotification", notification);
        }

        public async Task SendBookingConfirmationToCustomerAsync(string userId, string bookingId, string roomName, string message)
        {
            var notification = new
            {
                message,
                type = "booking_confirmation",
                timestamp = DateTime.Now.ToString("o"), // ISO 8601 format
                data = new
                {
                    bookingId,
                    roomName,
                    action = "new_booking_confirmation"
                }
            };

            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendBookingStatusUpdateToCustomerAsync(string userId, string bookingId, string status, string message)
        {
            var notification = new
            {
                message = message,
                type = "booking_status",
                timestamp = DateTime.Now.ToString("o"), // ISO 8601 format
                data = new
                {
                    bookingId,
                    status,
                    action = "status_update"
                }
            };

            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendPaymentNotificationAsync(string userId, string bookingId, string paymentStatus, string message)
        {
            var notification = new
            {
                message = message,
                type = "payment",
                timestamp = DateTime.Now.ToString("o"), // ISO 8601 format
                data = new
                {
                    bookingId,
                    paymentStatus,
                    action = "payment_update"
                }
            };

            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendGeneralNotificationToUserAsync(string userId, string message, string type = "info")
        {
            var notification = new
            {
                message = message,
                type = type,
                timestamp = DateTime.Now
            };

            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendGeneralNotificationToAdminsAsync(string message, string type = "info", object? data = null)
        {
            var notification = new
            {
                message = message,
                type = type,
                timestamp = DateTime.Now,
                data = data
            };

            await _hubContext.Clients.Group("AdminGroup").SendAsync("ReceiveAdminNotification", notification);
        }

        public async Task SendBookingCancellationToAdminAsync(int bookingId, string customerName, string roomName, string reason)
        {
            var message = $"Khách hàng {customerName} đã hủy đặt phòng #{bookingId} - {roomName}";

            var notification = new
            {
                message = message,
                type = "cancellation",
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                data = new
                {
                    bookingId = bookingId,
                    customerName = customerName,
                    roomName = roomName,
                    action = "customer_cancelled",
                    reason = reason
                }
            };

            await _hubContext.Clients.Group("AdminGroup").SendAsync("ReceiveAdminNotification", notification);
        }

        public async Task SendReviewNotificationToAdminAsync(int bookingId, string customerName, string roomName, int rating, string comment)
        {
            var message = $"Khách hàng {customerName} đã đánh giá {rating} sao cho phòng {roomName}";

            var notification = new
            {
                message = message,
                type = "review",
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                data = new
                {
                    bookingId = bookingId,
                    customerName = customerName,
                    roomName = roomName,
                    action = "customer_reviewed",
                    rating = rating,
                    comment = comment
                }
            };

            await _hubContext.Clients.Group("AdminGroup").SendAsync("ReceiveAdminNotification", notification);
        }
    }
}
