using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HotelBookingSystem.Services.Implementations
{
    public interface IAdminRoomService
    {
        Task<RoomsQueryViewModel> GetRooms(RoomsQueryViewModel query);
        Task<RoomDetailsViewModel?> GetById(int id, CancellationToken ct);
        Task<Room> Add(CreateRoomViewModel model, CancellationToken ct);
        Task<Room?> Update(EditRoomViewModel model, CancellationToken ct);
        Task<bool> DeactivateRoom(int id);
        Task<bool> ActivateRoom(int id);
    }
    public class AdminRoomService : IAdminRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageStorageService _imageStorage;
        public AdminRoomService(ApplicationDbContext context, IImageStorageService imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        public async Task<RoomsQueryViewModel> GetRooms(RoomsQueryViewModel query)
        {
            var roomsQuery = _context.Rooms.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                roomsQuery = roomsQuery.Where(r => r.Name.Contains(query.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(query.RoomType))
            {
                roomsQuery = roomsQuery.Where(r => r.RoomType == query.RoomType);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                roomsQuery = query.Status.ToLower() switch
                {
                    "available" => roomsQuery.Where(r => r.IsAvailable && r.IsActivated),
                    "unavailable" => roomsQuery.Where(r => !r.IsAvailable && r.IsActivated),
                    "deactivated" => roomsQuery.Where(r => !r.IsActivated),
                    "activated" => roomsQuery.Where(r => r.IsActivated),
                    _ => roomsQuery
                };
            }

            roomsQuery = query.SortBy switch
            {
                "name" => query.SortDescending
                            ? roomsQuery.OrderByDescending(r => r.Name)
                            : roomsQuery.OrderBy(r => r.Name),
                "price" => query.SortDescending
                            ? roomsQuery.OrderByDescending(r => r.PricePerNight)
                            : roomsQuery.OrderBy(r => r.PricePerNight),
                "type" => query.SortDescending
                            ? roomsQuery.OrderByDescending(r => r.RoomType)
                            : roomsQuery.OrderBy(r => r.RoomType),
                "capacity" => query.SortDescending
                            ? roomsQuery.OrderByDescending(r => r.Capacity)
                            : roomsQuery.OrderBy(r => r.Capacity),
                _ => roomsQuery.OrderBy(r => r.Id)
            };

            var totalCount = await roomsQuery.CountAsync();

            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;

            var rooms = await roomsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            query.AvailableCount = await _context.Rooms.CountAsync(r => r.IsAvailable);
            query.OccupiedCount = await _context.Rooms.CountAsync(r => !r.IsAvailable);
            query.ActivatedCount = await _context.Rooms.CountAsync(r => r.IsActivated);
            query.DeactivatedCount = await _context.Rooms.CountAsync(r => !r.IsActivated);

            query.Rooms = rooms;
            query.TotalCount = totalCount;

            return query;
        }

        public async Task<RoomDetailsViewModel?> GetById(int id, CancellationToken ct = default)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (room == null) return null;

            return new RoomDetailsViewModel
            {
                Room = new EditRoomViewModel
                {
                    Id = room.Id,
                    Name = room.Name,
                    RoomType = room.RoomType,
                    PricePerNight = room.PricePerNight,
                    Capacity = room.Capacity,
                    Description = room.Description,
                    IsAvailable = room.IsAvailable,
                    IsActivated = room.IsActivated,
                    ImageUrl = room.ImageUrl,
                    ImagePublicId = room.ImagePublicId
                }
            };
        }

        public async Task<Room> Add(CreateRoomViewModel model, CancellationToken ct = default)
        {
            var upload = await _imageStorage.UploadRoomImage(model.ImageFile, ct);

            var room = new Room
            {
                Name = model.Name,
                RoomType = model.RoomType,
                PricePerNight = model.PricePerNight,
                Capacity = model.Capacity,
                Description = model.Description,
                IsAvailable = model.IsAvailable,
                IsActivated = model.IsActivated,
                ImageUrl = upload.Url,
                ImagePublicId = upload.PublicId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(ct);

            return room;
        }

        public async Task<Room?> Update(EditRoomViewModel model, CancellationToken ct = default)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == model.Id, ct);
            if (room == null) return null;

            // If new image uploaded → replace on Cloudinary
            if (model.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(room.ImagePublicId))
                {
                    await _imageStorage.Delete(room.ImagePublicId, ct);
                }

                var upload = await _imageStorage.UploadRoomImage(model.ImageFile, ct);
                room.ImageUrl = upload.Url;
                room.ImagePublicId = upload.PublicId;
            }

            room.Name = model.Name;
            room.RoomType = model.RoomType;
            room.PricePerNight = model.PricePerNight;
            room.Capacity = model.Capacity;
            room.Description = model.Description;
            room.IsAvailable = model.IsAvailable;
            room.IsActivated = model.IsActivated;

            await _context.SaveChangesAsync(ct);
            return room;
        }

        public async Task<bool> DeactivateRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return false;

            room.IsActivated = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return false;

            room.IsActivated = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
