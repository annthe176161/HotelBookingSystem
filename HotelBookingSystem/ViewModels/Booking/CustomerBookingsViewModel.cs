using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Booking
{
    public class CustomerBookingsViewModel
    {
        public string SearchTerm { get; set; } = "";
        public string Status { get; set; } = "";
        public string RoomType { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public List<CustomerBookingItem> Bookings { get; set; } = new List<CustomerBookingItem>();
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int PendingBookings { get; set; }
    }

    public class CustomerBookingItem
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Guests { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Room Information
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        public string RoomImageUrl { get; set; }
        public decimal RoomPricePerNight { get; set; }

        // Status Information
        public string Status { get; set; }
        public string StatusColor { get; set; }

        // Payment Information
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }

        // Calculated Properties
        public int NightCount => (CheckOut - CheckIn).Days;
        public bool CanCancel => Status == "Confirmed" && CheckIn > DateTime.Now.AddDays(1);
        public bool CanReview => Status == "Hoàn thành" && CompletedDate.HasValue && !HasReview;
        public bool HasReview { get; set; }
        public bool IsUpcoming => CheckIn > DateTime.Now && Status == "Confirmed";
        public bool IsActive => CheckIn <= DateTime.Now && CheckOut > DateTime.Now && Status == "Confirmed";
        public bool IsPast => CheckOut < DateTime.Now;

        public string FormattedBookingNumber => $"#BK{Id.ToString().PadLeft(6, '0')}";
        public string FormattedCheckIn => CheckIn.ToString("dd/MM/yyyy");
        public string FormattedCheckOut => CheckOut.ToString("dd/MM/yyyy");
        public string FormattedCreatedDate => CreatedDate.ToString("dd/MM/yyyy HH:mm");
        public string FormattedTotalPrice => TotalPrice.ToString("N0") + " VNĐ";
    }
}
