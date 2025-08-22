using HotelBookingSystem.Data;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Room;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomService> _logger;

        public RoomService(ApplicationDbContext context, ILogger<RoomService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RoomListViewModel> GetFilteredRoomsAsync(RoomListViewModel searchModel)
        {
            var query = _context.Rooms.Include(r => r.Reviews)
                .Where(r => r.IsAvailable == true && r.IsActivated == true)
                .AsQueryable();

            // Filtering logic
            // Only filter by guests if > 0, otherwise show all rooms
            if (searchModel.Guests > 0)
            {
                query = query.Where(r => r.Capacity >= searchModel.Guests);
            }
            if (!string.IsNullOrEmpty(searchModel.RoomType))
            {
                query = query.Where(r => r.RoomType == searchModel.RoomType);
            }

            // Sorting logic
            switch (searchModel.SortBy?.ToLower())
            {
                case "price-low": query = query.OrderBy(r => r.PricePerNight); break;
                case "price-high": query = query.OrderByDescending(r => r.PricePerNight); break;
                case "rating": query = query.OrderByDescending(r => r.AverageRating); break;
                default: query = query.OrderByDescending(r => r.AverageRating).ThenBy(r => r.PricePerNight); break;
            }

            var allMatchingRooms = await query.ToListAsync();
            var totalRooms = allMatchingRooms.Count;
            var totalPages = (int)Math.Ceiling(totalRooms / (double)searchModel.PageSize);

            // Ensure current page is within valid range
            if (searchModel.CurrentPage < 1)
                searchModel.CurrentPage = 1;
            if (searchModel.CurrentPage > totalPages && totalPages > 0)
                searchModel.CurrentPage = totalPages;

            var roomsForPage = allMatchingRooms
                .Skip((searchModel.CurrentPage - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .Select(room => new RoomCardViewModel
                {
                    Id = room.Id,
                    Name = room.Name,
                    Description = room.Description,
                    ImageUrl = room.ImageUrl,
                    RoomType = room.RoomType,
                    Capacity = room.Capacity,
                    PricePerNight = room.PricePerNight,
                    AverageRating = room.AverageRating,
                    ReviewsCount = room.Reviews?.Count ?? 0,
                    Amenities = new List<string> { "WiFi", "TV", "Điều hòa" }
                }).ToList();

            var viewModel = new RoomListViewModel
            {
                Rooms = roomsForPage,
                CurrentPage = searchModel.CurrentPage,
                TotalRooms = totalRooms,
                PageSize = searchModel.PageSize,
                TotalPages = totalPages,
                RoomTypes = await _context.Rooms.Select(r => r.RoomType).Distinct().ToListAsync()
            };

            // Copy search parameters back to the model
            viewModel.CheckInDate = searchModel.CheckInDate;
            viewModel.CheckOutDate = searchModel.CheckOutDate;
            viewModel.Guests = searchModel.Guests;
            viewModel.RoomType = searchModel.RoomType;
            viewModel.SortBy = searchModel.SortBy ?? "recommended";

            return viewModel;
        }

        public async Task<RoomDetailsViewModel> GetRoomDetailsAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                // Instead of returning null, throw an exception or handle as needed.
                // Here, we throw an exception to match the non-nullable return type.
                throw new InvalidOperationException($"Room with ID {roomId} not found.");
            }

            var similarRooms = await _context.Rooms
                .Where(r => r.RoomType == room.RoomType && r.Id != roomId)
                .Take(4)
                .Select(r => new SimilarRoomViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    ImageUrl = r.ImageUrl,
                    Price = r.PricePerNight,
                    Rating = r.AverageRating
                })
                .ToListAsync();

            var viewModel = new RoomDetailsViewModel
            {
                Id = room.Id,
                Name = room.Name,
                Description = room.Description,
                MainImageUrl = room.ImageUrl,
                ImageUrls = new List<string> { room.ImageUrl },
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                AverageRating = room.AverageRating,
                ReviewsCount = room.Reviews.Count,
                Reviews = room.Reviews.Select(rev => new ReviewViewModel
                {
                    UserName = "Anonymous",
                    Rating = rev.Rating,
                    Comment = rev.Comment,
                    Date = rev.CreatedDate
                }).ToList(),
                Features = new List<RoomFeatureViewModel>
        {
            new() { Name = "WiFi miễn phí", Icon = "fas fa-wifi" },
            new() { Name = "Điều hòa", Icon = "fas fa-snowflake" },
            new() { Name = "TV màn hình phẳng", Icon = "fas fa-tv" },
            new() { Name = "Minibar", Icon = "fas fa-glass-martini" },
        },
                SimilarRooms = similarRooms
            };

            return viewModel;
        }

        public async Task<List<RoomCardViewModel>> GetFeaturedRoomsAsync(int count)
        {
            return await _context.Rooms
                .Where(r => r.IsAvailable == true && r.IsActivated == true)
                .OrderByDescending(r => r.AverageRating)
                .Take(count)
                .Select(r => new RoomCardViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    AverageRating = r.AverageRating
                })
                .ToListAsync();
        }
    }
}
