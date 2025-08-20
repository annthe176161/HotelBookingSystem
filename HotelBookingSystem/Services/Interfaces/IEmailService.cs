using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendBookingConfirmationToCustomerAsync(Booking booking);
        Task SendBookingNotificationToHotelAsync(Booking booking);
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
