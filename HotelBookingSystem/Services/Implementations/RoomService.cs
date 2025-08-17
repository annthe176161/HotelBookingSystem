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
            var query = _context.Rooms.Include(r => r.Reviews).AsQueryable();

            // Filtering logic (as implemented before)
            int totalGuests = searchModel.Adults + searchModel.Children;
            if (totalGuests > 1) query = query.Where(r => r.Capacity >= totalGuests);
            if (!string.IsNullOrEmpty(searchModel.RoomType)) query = query.Where(r => r.RoomType == searchModel.RoomType);
            if (searchModel.MinPrice.HasValue) query = query.Where(r => r.PricePerNight >= searchModel.MinPrice.Value);
            if (searchModel.MaxPrice.HasValue) query = query.Where(r => r.PricePerNight <= searchModel.MaxPrice.Value);

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
                TotalPages = (int)Math.Ceiling(totalRooms / (double)searchModel.PageSize),
                RoomTypes = await _context.Rooms.Select(r => r.RoomType).Distinct().ToListAsync(),
                LowestPrice = allMatchingRooms.Any() ? allMatchingRooms.Min(r => r.PricePerNight) : 0,
                HighestPrice = allMatchingRooms.Any() ? allMatchingRooms.Max(r => r.PricePerNight) : 0
            };

            // Copy search parameters back to the model
            viewModel.CheckInDate = searchModel.CheckInDate;
            viewModel.CheckOutDate = searchModel.CheckOutDate;
            viewModel.Adults = searchModel.Adults;
            viewModel.Children = searchModel.Children;
            viewModel.RoomType = searchModel.RoomType;
            viewModel.MinPrice = searchModel.MinPrice ?? viewModel.LowestPrice;
            viewModel.MaxPrice = searchModel.MaxPrice ?? viewModel.HighestPrice;
            viewModel.SortBy = searchModel.SortBy;

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
