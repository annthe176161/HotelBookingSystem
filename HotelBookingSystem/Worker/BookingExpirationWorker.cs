using HotelBookingSystem.Data;
using HotelBookingSystem.Services.Interfaces;

namespace HotelBookingSystem.Worker
{
    public class BookingExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingExpirationWorker> _logger;

        public BookingExpirationWorker(
            IServiceProvider serviceProvider,
            ILogger<BookingExpirationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingExpirationWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var bookingStatusService = scope.ServiceProvider.GetRequiredService<IBookingStatusService>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var expirationTime = DateTime.Now.AddMinutes(-0.5);

                        // Tìm các booking chờ xác nhận quá 5 phút
                        var expiredBookings = dbContext.Bookings
                            .Where(b => b.BookingStatusId == 1 && b.CreatedDate <= expirationTime)
                            .ToList();

                        foreach (var booking in expiredBookings)
                        {
                            _logger.LogInformation($"Booking {booking.Id} expired -> auto cancel.");
                            await bookingStatusService.CancelBookingAsync(booking.Id, "Tự động hủy vì quá hạn xác nhận");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing expired bookings.");
                }

                // Chạy lại sau 1 phút
                await Task.Delay(TimeSpan.FromMinutes(0.5), stoppingToken);
            }
        }
    }
}