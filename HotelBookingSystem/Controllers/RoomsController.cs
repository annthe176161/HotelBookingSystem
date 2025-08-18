using HotelBookingSystem.Data;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Room;
using HotelBookingSystem.ViewModels.RoomsViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index([FromQuery] RoomListViewModel searchModel)
        {
            // Set default values if not provided
            searchModel.CurrentPage = searchModel.CurrentPage > 0 ? searchModel.CurrentPage : 1;
            searchModel.PageSize = searchModel.PageSize > 0 ? searchModel.PageSize : 9;
            searchModel.Guests = searchModel.Guests > 0 ? searchModel.Guests : 1;

            var viewModel = await _roomService.GetFilteredRoomsAsync(searchModel);
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var roomDetails = await _roomService.GetRoomDetailsAsync(id);
                return View(roomDetails);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
}
