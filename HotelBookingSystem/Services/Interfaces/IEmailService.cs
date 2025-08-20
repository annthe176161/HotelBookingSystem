using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services.Interfaces
{
    public interface IEmailService
    {
        // Email đặt phòng ban đầu
        Task SendBookingConfirmationToCustomerAsync(Booking booking);
        Task SendBookingNotificationToHotelAsync(Booking booking);

        // Email thay đổi trạng thái đặt phòng
        Task SendBookingStatusChangeToCustomerAsync(Booking booking, string oldStatus, string newStatus);
        Task SendBookingStatusChangeToHotelAsync(Booking booking, string oldStatus, string newStatus);

        // Email thay đổi trạng thái thanh toán
        Task SendPaymentStatusChangeToCustomerAsync(Booking booking, string oldPaymentStatus, string newPaymentStatus);
        Task SendPaymentStatusChangeToHotelAsync(Booking booking, string oldPaymentStatus, string newPaymentStatus);

        // Email hủy đặt phòng
        Task SendBookingCancellationToCustomerAsync(Booking booking, string reason);
        Task SendBookingCancellationToHotelAsync(Booking booking, string reason);

        // Email nhắc nhở
        Task SendCheckInReminderToCustomerAsync(Booking booking);
        Task SendPaymentReminderToCustomerAsync(Booking booking);

        // Email chung
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
