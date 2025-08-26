using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Booking
{
    public class BookingReviewViewModel
    {
        // Thông tin booking
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = "";
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        // Thông tin phòng
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public string RoomType { get; set; } = "";
        public string RoomImageUrl { get; set; } = "";

        // Thông tin đánh giá
        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        public string? Comment { get; set; } = "";
        public DateTime ReviewDate { get; set; }

        // Navigation
        public bool CanEdit { get; set; }
    }
}
