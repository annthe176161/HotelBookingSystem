namespace HotelBookingSystem.ViewModels.Room
{
    public class RoomListViewModel
    {
        public List<RoomCardViewModel> Rooms { get; set; } = new List<RoomCardViewModel>();

        // Filter & Search Parameters
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int Guests { get; set; } = 1;
        public string? RoomType { get; set; }
        public string SortBy { get; set; } = "recommended";

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 6; // Số phòng trên mỗi trang
        public int TotalRooms { get; set; }

        // Data for filter controls
        public List<string> RoomTypes { get; set; } = new List<string>();
    }
}
