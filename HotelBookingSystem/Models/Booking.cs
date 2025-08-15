namespace HotelBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Guests { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }

        public int BookingStatusId { get; set; }
        public virtual BookingStatus BookingStatus { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        public virtual Payment? Payment { get; set; }
        public virtual Review? Review { get; set; }
    }
}
