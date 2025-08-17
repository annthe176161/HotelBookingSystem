using HotelBookingSystem.ViewModels.Room;

namespace HotelBookingSystem.Services.Interfaces
{
    public interface IRoomService
    {
        Task<RoomListViewModel> GetFilteredRoomsAsync(RoomListViewModel searchModel);
        Task<RoomDetailsViewModel> GetRoomDetailsAsync(int roomId);
        Task<List<RoomCardViewModel>> GetFeaturedRoomsAsync(int count);
    }
}
