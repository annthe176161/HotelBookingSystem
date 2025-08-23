using HotelBookingSystem.ViewModels.Admin;

namespace HotelBookingSystem.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardViewModel> GetDashboardData(DateTime startDate, DateTime endDate);
    }
}