using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Booking
{
    public class BookingViewModel
    {
        public int RoomId { get; set; }

        public string RoomName { get; set; } = "";

        public string RoomType { get; set; } = "";

        public string RoomImageUrl { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        [Display(Name = "Ngày nhận phòng")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        [Display(Name = "Ngày trả phòng")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số khách")]
        [Range(1, 10, ErrorMessage = "Số khách phải từ 1 đến 10 người")]
        [Display(Name = "Số khách")]
        public int? GuestCount { get; set; }

        [Display(Name = "Yêu cầu đặc biệt")]
        public string? SpecialRequests { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập họ")]
        [Display(Name = "Họ")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = "";

        // Calculated properties
        public int NightCount => (CheckOutDate - CheckInDate).Days;

        public int MaxGuests { get; set; }

        public decimal RoomPrice { get; set; }

        public decimal TotalPrice => RoomPrice * NightCount;
    }
}
