using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public class BookingStatusService : IBookingStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingStatusService> _logger;

        public BookingStatusService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<BookingStatusService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task UpdateBookingStatusAsync(int bookingId, int newStatusId, string reason = "")
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.BookingStatus)
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    throw new ArgumentException($"Không tìm thấy đặt phòng với ID: {bookingId}");
                }

                var newStatus = await _context.BookingStatuses.FindAsync(newStatusId);
                if (newStatus == null)
                {
                    throw new ArgumentException($"Không tìm thấy trạng thái với ID: {newStatusId}");
                }

                var oldStatusName = booking.BookingStatus?.Name ?? "Không xác định";
                var newStatusName = newStatus.Name;

                // Cập nhật trạng thái
                booking.BookingStatusId = newStatusId;
                booking.BookingStatus = newStatus;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Gửi email thông báo
                await _emailService.SendBookingStatusChangeToCustomerAsync(booking, oldStatusName, newStatusName);
                await _emailService.SendBookingStatusChangeToHotelAsync(booking, oldStatusName, newStatusName);

                _logger.LogInformation($"Updated booking {bookingId} status from {oldStatusName} to {newStatusName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating booking status for booking {bookingId}");
                throw;
            }
        }

        public async Task UpdatePaymentStatusAsync(int bookingId, int newPaymentStatusId, string reason = "")
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Payment)
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking?.Payment != null)
                {
                    await _context.Entry(booking.Payment)
                        .Reference(p => p.PaymentStatus)
                        .LoadAsync();
                }

                if (booking == null)
                {
                    throw new ArgumentException($"Không tìm thấy đặt phòng với ID: {bookingId}");
                }

                var newPaymentStatus = await _context.PaymentStatuses.FindAsync(newPaymentStatusId);
                if (newPaymentStatus == null)
                {
                    throw new ArgumentException($"Không tìm thấy trạng thái thanh toán với ID: {newPaymentStatusId}");
                }

                var oldPaymentStatusName = booking.Payment?.PaymentStatus?.Name ?? "Chưa có thanh toán";
                var newPaymentStatusName = newPaymentStatus.Name;

                // Tạo payment nếu chưa có
                if (booking.Payment == null)
                {
                    booking.Payment = new Payment
                    {
                        BookingId = booking.Id,
                        Amount = booking.TotalPrice,
                        PaymentMethod = "Chưa xác định",
                        TransactionId = $"TXN_{booking.Id}_{DateTime.Now:yyyyMMddHHmmss}",
                        PaymentDate = DateTime.Now,
                        PaymentStatusId = newPaymentStatusId
                    };
                    _context.Payments.Add(booking.Payment);
                }
                else
                {
                    // Cập nhật payment status
                    booking.Payment.PaymentStatusId = newPaymentStatusId;
                    booking.Payment.PaymentStatus = newPaymentStatus;
                }

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Gửi email thông báo
                await _emailService.SendPaymentStatusChangeToCustomerAsync(booking, oldPaymentStatusName, newPaymentStatusName);
                await _emailService.SendPaymentStatusChangeToHotelAsync(booking, oldPaymentStatusName, newPaymentStatusName);

                _logger.LogInformation($"Updated payment status for booking {bookingId} from {oldPaymentStatusName} to {newPaymentStatusName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating payment status for booking {bookingId}");
                throw;
            }
        }

        public async Task CancelBookingAsync(int bookingId, string reason)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.BookingStatus)
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    throw new ArgumentException($"Không tìm thấy đặt phòng với ID: {bookingId}");
                }

                // Tìm trạng thái "Cancelled" hoặc "Hủy"
                var cancelledStatus = await _context.BookingStatuses
                    .FirstOrDefaultAsync(s => s.Name.ToLower().Contains("cancel") || s.Name.ToLower().Contains("hủy"));

                if (cancelledStatus == null)
                {
                    // Tạo trạng thái hủy nếu chưa có
                    cancelledStatus = new BookingStatus
                    {
                        Name = "Đã hủy",
                        Description = "Đặt phòng đã bị hủy"
                    };
                    _context.BookingStatuses.Add(cancelledStatus);
                    await _context.SaveChangesAsync();
                }

                // Cập nhật trạng thái
                booking.BookingStatusId = cancelledStatus.Id;
                booking.BookingStatus = cancelledStatus;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Gửi email thông báo hủy
                await _emailService.SendBookingCancellationToCustomerAsync(booking, reason);
                await _emailService.SendBookingCancellationToHotelAsync(booking, reason);

                _logger.LogInformation($"Cancelled booking {bookingId} with reason: {reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling booking {bookingId}");
                throw;
            }
        }

        public async Task SendCheckInReminderAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    throw new ArgumentException($"Không tìm thấy đặt phòng với ID: {bookingId}");
                }

                await _emailService.SendCheckInReminderToCustomerAsync(booking);
                _logger.LogInformation($"Sent check-in reminder for booking {bookingId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending check-in reminder for booking {bookingId}");
                throw;
            }
        }

        public async Task SendPaymentReminderAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking?.Payment != null)
                {
                    await _context.Entry(booking.Payment)
                        .Reference(p => p.PaymentStatus)
                        .LoadAsync();
                }

                if (booking == null)
                {
                    throw new ArgumentException($"Không tìm thấy đặt phòng với ID: {bookingId}");
                }

                // Chỉ gửi nhắc nhở nếu chưa thanh toán hoặc thanh toán thất bại
                if (booking.Payment == null ||
                    booking.Payment.PaymentStatus?.Name?.ToLower().Contains("pending") == true ||
                    booking.Payment.PaymentStatus?.Name?.ToLower().Contains("chờ") == true ||
                    booking.Payment.PaymentStatus?.Name?.ToLower().Contains("failed") == true ||
                    booking.Payment.PaymentStatus?.Name?.ToLower().Contains("thất bại") == true)
                {
                    await _emailService.SendPaymentReminderToCustomerAsync(booking);
                    _logger.LogInformation($"Sent payment reminder for booking {bookingId}");
                }
                else
                {
                    _logger.LogInformation($"No payment reminder needed for booking {bookingId} - already paid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending payment reminder for booking {bookingId}");
                throw;
            }
        }
    }
}
