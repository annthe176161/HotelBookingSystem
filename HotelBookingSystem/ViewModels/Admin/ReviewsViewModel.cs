namespace HotelBookingSystem.ViewModels.Admin
{
    public class ReviewsQueryViewModel
    {
        public List<HotelBookingSystem.Models.Review>? Reviews { get; set; }
        public List<HotelBookingSystem.Models.Room>? Rooms { get; set; }

        // Input (filters/search/sort)
        public int? Rating { get; set; }
        public int? RoomId { get; set; }
        public string CreateDateSort { get; set; } = "desc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public int TotalReviews { get; set; }
        public int PositiveCount { get; set; }
        public int NegativeCount { get; set; }

    }
}
