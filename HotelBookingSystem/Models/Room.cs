namespace HotelBookingSystem.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal PricePerNight { get; set; }
        public string ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
        public int Capacity { get; set; }
        public string RoomType { get; set; }
        public bool IsAvailable { get; set; } = true;
        public double AverageRating { get; set; } = 0;
        public bool IsActivated { get; set; } = true;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
