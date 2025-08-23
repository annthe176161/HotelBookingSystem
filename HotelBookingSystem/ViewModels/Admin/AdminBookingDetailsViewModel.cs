namespace HotelBookingSystem.ViewModels.Admin
{
    public class AdminBookingDetailsViewModel
    {
        // Booking Information
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Guests { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SpecialRequests { get; set; } = string.Empty;

        // Customer Information
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime CustomerJoinDate { get; set; }

        // Room Information
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string RoomDescription { get; set; } = string.Empty;
        public string RoomImageUrl { get; set; } = string.Empty;
        public decimal RoomPricePerNight { get; set; }
        public int RoomCapacity { get; set; }
        public string RoomBedType { get; set; } = string.Empty;
        public string RoomFloor { get; set; } = string.Empty;
        public string RoomBuilding { get; set; } = string.Empty;

        // Payment Information
        public int? PaymentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }

        // Review Information
        public bool HasReview { get; set; }
        public int? ReviewRating { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public DateTime? ReviewDate { get; set; }

        // Calculated Properties
        public int NightCount => (CheckOut - CheckIn).Days;
        public bool CanUpdate => Status != "Hoàn thành" && Status != "Đã hủy";
        public bool CanComplete => Status == "Đã xác nhận" && PaymentStatus == "Thành công";
        public bool CanRefund => PaymentStatus == "Thành công";
        public string StatusColor => Status switch
        {
            "Chờ xác nhận" => "warning",
            "Đã xác nhận" => "primary",
            "Hoàn thành" => "success",
            "Đã hủy" => "danger",
            _ => "secondary"
        };
        public string PaymentStatusColor => PaymentStatus switch
        {
            "Đang xử lý" => "warning",
            "Thành công" => "success",
            "Thất bại" => "danger",
            "Đã hoàn tiền" => "info",
            _ => "secondary"
        };
    }
}
