using System;
using System.Collections.Generic;

namespace HotelBookingSystem.ViewModels.Admin
{
    public class DashboardViewModel
    {
        // Key Metrics
        public decimal TotalRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int TotalBookings { get; set; }
        public decimal BookingGrowth { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal OccupancyGrowth { get; set; }
        public int NewCustomers { get; set; }
        public decimal CustomerGrowth { get; set; }

        // Revenue Chart Data
        public List<decimal> MonthlyRevenue { get; set; } = new();
        public List<string> RevenueLabels { get; set; } = new();

        // Room Types Distribution
        public Dictionary<string, int> RoomTypeBookings { get; set; } = new();

        // Room Status
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int CleaningRooms { get; set; }
        public int MaintenanceRooms { get; set; }

        // Recent Bookings
        public List<RecentBookingViewModel> RecentBookings { get; set; } = new();

        // Recent Reviews
        public List<RecentReviewViewModel> RecentReviews { get; set; } = new();

        // Top Customers
        public List<TopCustomerViewModel> TopCustomers { get; set; } = new();

        // Recent Notifications
        public List<RecentNotificationViewModel> RecentNotifications { get; set; } = new();
    }

    public class RecentBookingViewModel
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }

    public class RecentReviewViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerInitials { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class TopCustomerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class RecentNotificationViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsUnread { get; set; }
    }
}