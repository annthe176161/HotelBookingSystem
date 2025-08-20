using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.ViewModels.Admin
{
    public class CreateRoomViewModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [Required]
        public string RoomType { get; set; } = default!;

        [Required, Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }

        [Required, Range(1, 20)]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        public bool IsActivated { get; set; } = true;

        [Required]
        public IFormFile ImageFile { get; set; } = default!;
    }

    public class EditRoomViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [Required]
        public string RoomType { get; set; } = default!;

        [Required, Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }

        [Required, Range(1, 20)]
        public int Capacity { get; set; }

        public string? Description { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public bool IsActivated { get; set; } = true;

        // For display of existing image
        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }

        // Optional new upload
        public IFormFile? ImageFile { get; set; }
    }

    public class RoomDetailsViewModel
    {
        public EditRoomViewModel Room { get; set; } = default!;
    }

    public class RoomsQueryViewModel
    {
        public List<HotelBookingSystem.Models.Room>? Rooms { get; set; }

        // Input (filters/search/sort)
        public string? SearchTerm { get; set; }
        public string? RoomType { get; set; }
        public string? Status { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public int AvailableCount { get; set; }
        public int OccupiedCount { get; set; }
        public int ActivatedCount { get; set; }
        public int DeactivatedCount { get; set; }
        public int TotalRooms => ActivatedCount + DeactivatedCount;
    }
}
