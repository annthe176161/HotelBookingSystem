namespace HotelBookingSystem.ViewModels.Room
{
    public class RoomCardViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string ImageUrl { get; set; }
        public required string RoomType { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();
    }
}
