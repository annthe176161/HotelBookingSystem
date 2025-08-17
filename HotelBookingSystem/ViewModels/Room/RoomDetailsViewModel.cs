namespace HotelBookingSystem.ViewModels.Room
{
    // ViewModel cho một tiện nghi
    public class RoomFeatureViewModel
    {
        public required string Name { get; set; }
        public required string Icon { get; set; }
    }

    // ViewModel cho một đánh giá
    public class ReviewViewModel
    {
        public required string UserName { get; set; }
        public string? UserAvatarUrl { get; set; }
        public int Rating { get; set; }
        public DateTime Date { get; set; }
        public required string Comment { get; set; }
    }

    // ViewModel cho một phòng tương tự
    public class SimilarRoomViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public double Rating { get; set; }
    }

    // ViewModel chính cho trang chi tiết phòng
    public class RoomDetailsViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string MainImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public required string RoomType { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public List<RoomFeatureViewModel> Features { get; set; } = new();
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public List<SimilarRoomViewModel> SimilarRooms { get; set; } = new();
    }
}
