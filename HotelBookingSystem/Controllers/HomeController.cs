using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Room;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HotelBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRoomService _roomService;

        public HomeController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredRooms = await _roomService.GetFeaturedRoomsAsync(4);
            return View(featuredRooms);
        }

        public async Task<IActionResult> Rooms(RoomListViewModel searchModel, int page = 1)
        {
            // Set the current page from parameter
            searchModel.CurrentPage = page;

            // Normalize empty strings to null for proper filtering
            if (string.IsNullOrWhiteSpace(searchModel.RoomType))
            {
                searchModel.RoomType = null;
            }

            // The controller just calls the service. All logic is in the service.
            var viewModel = await _roomService.GetFilteredRoomsAsync(searchModel);
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
