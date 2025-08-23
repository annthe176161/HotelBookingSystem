using HotelBookingSystem.Data;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardData(DateTime startDate, DateTime endDate)
        {
            var lastMonthStart = startDate.AddMonths(-1);
            var lastMonthEnd = endDate.AddMonths(-1);

            // Current Period Metrics
            var currentBookings = await GetBookingsInPeriod(startDate, endDate);
            var lastMonthBookings = await GetBookingsInPeriod(lastMonthStart, lastMonthEnd);

            var currentRevenue = currentBookings.Where(b => b.Payment != null && b.Payment.PaymentStatus.Name == "Thành công")
                                              .Sum(b => b.TotalPrice);
            var lastMonthRevenue = lastMonthBookings.Where(b => b.Payment != null && b.Payment.PaymentStatus.Name == "Thành công")
                                                  .Sum(b => b.TotalPrice);

            var currentNewUsers = await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .CountAsync();
            var lastMonthNewUsers = await _context.Users
                .Where(u => u.CreatedAt >= lastMonthStart && u.CreatedAt <= lastMonthEnd)
                .CountAsync();

            // Calculate growth percentages
            var revenueGrowth = CalculateGrowthPercentage(lastMonthRevenue, currentRevenue);
            var bookingGrowth = CalculateGrowthPercentage(lastMonthBookings.Count, currentBookings.Count);
            var customerGrowth = CalculateGrowthPercentage(lastMonthNewUsers, currentNewUsers);

            // Room occupancy
            var totalRooms = await _context.Rooms.CountAsync(r => r.IsActivated);
            var occupiedRooms = await _context.Rooms.CountAsync(r => !r.IsAvailable);
            var occupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

            // Get monthly revenue data for the chart (last 12 months)
            var monthlyRevenue = new List<decimal>();
            var monthLabels = new List<string>();
            var endMonth = endDate.Date;
            var startMonth = endMonth.AddMonths(-11);

            while (startMonth <= endMonth)
            {
                var monthRevenue = await _context.Bookings
                    .Where(b => b.Payment != null && 
                               b.Payment.PaymentStatus.Name == "Thành công" &&
                               b.CreatedDate.Month == startMonth.Month &&
                               b.CreatedDate.Year == startMonth.Year)
                    .SumAsync(b => b.TotalPrice);

                monthlyRevenue.Add(monthRevenue);
                monthLabels.Add(startMonth.ToString("MM/yyyy"));
                startMonth = startMonth.AddMonths(1);
            }

            // Room type distribution
            var roomTypeBookings = await _context.Bookings
                .Where(b => b.CreatedDate >= startDate && b.CreatedDate <= endDate)
                .GroupBy(b => b.Room.RoomType)
                .Select(g => new { RoomType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoomType, x => x.Count);

            // Recent bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .OrderByDescending(b => b.CreatedDate)
                .Take(5)
                .Select(b => new RecentBookingViewModel
                {
                    Id = b.Id,
                    BookingNumber = $"B{b.Id.ToString().PadLeft(3, '0')}-{b.CreatedDate.Year}",
                    CustomerName = b.User.FullName ?? "",
                    RoomName = b.Room.Name,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Status = b.BookingStatus.Name,
                    TotalPrice = b.TotalPrice
                })
                .ToListAsync();

            // Recent reviews (project to anonymous, then to viewmodel with initials)
            var recentReviewsRaw = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CreatedDate)
                .Take(3)
                .Select(r => new {
                    CustomerName = r.User.FullName ?? "",
                    Rating = r.Rating,
                    RoomName = r.Room.Name,
                    ReviewDate = r.CreatedDate,
                    Comment = r.Comment
                })
                .ToListAsync();
            var recentReviews = recentReviewsRaw
                .Select(r => new RecentReviewViewModel {
                    CustomerName = r.CustomerName,
                    CustomerInitials = GetInitials(r.CustomerName),
                    Rating = r.Rating,
                    RoomName = r.RoomName,
                    ReviewDate = r.ReviewDate,
                    Comment = r.Comment
                }).ToList();

            // Top customers (project to anonymous, then to viewmodel with initials)
            var topCustomersRaw = await _context.Users
                .Where(u => u.Bookings.Any())
                .Select(u => new {
                    Name = u.FullName ?? "",
                    Email = u.Email ?? "",
                    TotalBookings = u.Bookings.Count,
                    TotalSpent = u.Bookings
                        .Where(b => b.Payment != null && b.Payment.PaymentStatus.Name == "Thành công")
                        .Sum(b => b.TotalPrice)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(5)
                .ToListAsync();
            var topCustomers = topCustomersRaw
                .Select(u => new TopCustomerViewModel {
                    Name = u.Name,
                    Email = u.Email,
                    Initials = GetInitials(u.Name),
                    TotalBookings = u.TotalBookings,
                    TotalSpent = u.TotalSpent
                }).ToList();

            // Room status counts
            var availableRooms = await _context.Rooms.CountAsync(r => r.IsAvailable && r.IsActivated);
            var cleaningRooms = 5; // Placeholder - add a status field to Room if needed
            var maintenanceRooms = 2; // Placeholder - add a status field to Room if needed

            return new DashboardViewModel
            {
                TotalRevenue = currentRevenue,
                RevenueGrowth = revenueGrowth,
                TotalBookings = currentBookings.Count,
                BookingGrowth = bookingGrowth,
                OccupancyRate = occupancyRate,
                OccupancyGrowth = 0, // Need historical data to calculate
                NewCustomers = currentNewUsers,
                CustomerGrowth = customerGrowth,
                MonthlyRevenue = monthlyRevenue,
                RevenueLabels = monthLabels,
                RoomTypeBookings = roomTypeBookings,
                AvailableRooms = availableRooms,
                OccupiedRooms = occupiedRooms,
                CleaningRooms = cleaningRooms,
                MaintenanceRooms = maintenanceRooms,
                RecentBookings = recentBookings,
                RecentReviews = recentReviews,
                TopCustomers = topCustomers,
                // Notifications would typically come from a separate notification service
                RecentNotifications = GetPlaceholderNotifications()
            };
        }

        private async Task<List<Models.Booking>> GetBookingsInPeriod(DateTime start, DateTime end)
        {
            return await _context.Bookings
                .Include(b => b.Payment)
                    .ThenInclude(p => p.PaymentStatus)
                .Where(b => b.CreatedDate >= start && b.CreatedDate <= end)
                .ToListAsync();
        }

        private decimal CalculateGrowthPercentage(decimal previous, decimal current)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return ((current - previous) / previous) * 100;
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "??";

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0].Substring(0, 2).ToUpper();
            return parts[0][0].ToString().ToUpper() + "?";
        }

        private List<RecentNotificationViewModel> GetPlaceholderNotifications()
        {
            // In a real application, these would come from a notification service
            return new List<RecentNotificationViewModel>
            {
                new() {
                    Title = "??t phòng m?i",
                    Message = "Có m?t ??t phòng m?i c?n xác nh?n",
                    Time = DateTime.Now.AddMinutes(-5),
                    Type = "booking",
                    IsUnread = true
                },
                new() {
                    Title = "?ánh giá m?i",
                    Message = "Khách hàng v?a ?ánh giá 5 sao",
                    Time = DateTime.Now.AddHours(-1),
                    Type = "review",
                    IsUnread = true
                },
                new() {
                    Title = "H?y ??t phòng",
                    Message = "M?t ??t phòng v?a b? h?y",
                    Time = DateTime.Now.AddHours(-2),
                    Type = "cancel",
                    IsUnread = false
                }
            };
        }
    }
}