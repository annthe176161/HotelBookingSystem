using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Booking
{
    public class BookingReviewViewModel
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        public string? Comment { get; set; } = "";
    }
}
