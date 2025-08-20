namespace HotelBookingSystem.ViewModels.Admin
{
    public class BookingQueryOptions
    {
        public int? BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BookingListItemViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = default!;
        public string CustomerEmail { get; set; } = default!;
        public string RoomName { get; set; } = default!;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Guests { get; set; }
        public decimal TotalPrice { get; set; }
        public string BookingStatus { get; set; } = default!;
        public string PaymentStatus { get; set; } = default!;
    }

    public class BookingsViewModel
    {
        public List<BookingListItemViewModel> Bookings { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalBookings { get; set; }
        public BookingQueryOptions Query { get; set; } = new();

        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
