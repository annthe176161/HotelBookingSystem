using HotelBookingSystem.Data;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{

    public interface IAdminReviewService
    {
        Task<ReviewsQueryViewModel> GetReviews(ReviewsQueryViewModel query);
    }

    public class AdminReviewService : IAdminReviewService
    {
        private readonly ApplicationDbContext _context;
        public AdminReviewService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ReviewsQueryViewModel> GetReviews(ReviewsQueryViewModel query)
        {
            var reviewsQuery = _context.Reviews
              .AsNoTracking()
              .Include(r => r.Room)   
              .Include(r => r.User)  
              .AsQueryable();

            if (query.Rating.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.Rating >= query.Rating.Value);
            }

            if (query.RoomId.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.RoomId == query.RoomId.Value);
            }

            var sort = (query.CreateDateSort ?? "desc").Trim().ToLowerInvariant();
            reviewsQuery = sort switch
            {
                "asc" => reviewsQuery.OrderBy(r => r.CreatedDate),
                "desc" => reviewsQuery.OrderByDescending(r => r.CreatedDate),
                _ => reviewsQuery.OrderByDescending(r => r.CreatedDate)
            };

            var totalCount = await reviewsQuery.CountAsync();

            var skip = (query.Page - 1) * query.PageSize;
            var reviews = await reviewsQuery
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            query.TotalReviews = await _context.Reviews.CountAsync();
            query.PositiveCount = await _context.Reviews.CountAsync(r => r.Rating >= 4);
            query.NegativeCount = await _context.Reviews.CountAsync(r => r.Rating < 4);

            query.Reviews = reviews;
            query.TotalCount = totalCount;

            // Take room list for dropdown
            query.Rooms = await _context.Rooms.AsNoTracking().OrderBy(r => r.Name).ToListAsync();

            return query;
        }

    }
}
