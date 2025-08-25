using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Infrastructure.Filters
{
    /// <summary>
    /// Bỏ qua kiểm tra trạng thái tài khoản cho controller/action này
    /// </summary>
    public class SkipAccountActiveCheckAttribute : Attribute
    {
    }
}
