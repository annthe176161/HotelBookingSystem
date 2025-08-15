namespace HotelBookingSystem.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
