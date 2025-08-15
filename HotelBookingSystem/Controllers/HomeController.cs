using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.ViewModels.RoomsViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HotelBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) // Update constructor
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var featuredRooms = _context.Rooms
                .Where(r => r.IsAvailable)
                .OrderByDescending(r => r.AverageRating)
                .Take(4)
                .ToList();

            return View(featuredRooms);
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

        public IActionResult Rooms(DateTime? checkin, DateTime? checkout, int adults = 2, int children = 0, int page = 1)
        {
            // M?u d? li?u cho UI
            var viewModel = new RoomsViewModel
            {
                CheckIn = checkin ?? DateTime.Today,
                CheckOut = checkout ?? DateTime.Today.AddDays(1),
                Adults = adults,
                Children = children,
                CurrentPage = page,
                TotalPages = 3, // Gi? l?p có 3 trang
                TotalRooms = 15 // Gi? l?p có 15 phòng
            };

            // T?o m?t s? phòng m?u ?? hi?n th? UI
            viewModel.Rooms = GetSampleRooms();

            return View(viewModel);
        }

        private List<RoomDisplayViewModel> GetSampleRooms()
        {
            // T?o danh sách các phòng m?u ?? hi?n th? UI
            return new List<RoomDisplayViewModel>
    {
        new RoomDisplayViewModel
        {
            Id = 1,
            Name = "Phòng Deluxe H??ng Bi?n",
            Description = "Phòng r?ng rãi v?i view bi?n tuy?t ??p, ???c trang b? ??y ?? ti?n nghi hi?n ??i.",
            ImageUrl = "/images/rooms/deluxe-ocean.jpg",
            RoomType = "Deluxe",
            BedType = "1 Gi??ng King",
            Size = 45,
            Capacity = 2,
            PricePerNight = 2000000,
            OriginalPrice = 2500000,
            IsPromotion = true,
            AverageRating = 4.8,
            ReviewsCount = 124,
            IsFavorited = false,
            IncludeBreakfast = true,
            Features = new List<RoomFeature>
            {
                new RoomFeature { Name = "WiFi mi?n phí", Icon = "fas fa-wifi" },
                new RoomFeature { Name = "?i?u hòa", Icon = "fas fa-snowflake" },
                new RoomFeature { Name = "Minibar", Icon = "fas fa-glass-martini" },
                new RoomFeature { Name = "TV màn hình ph?ng", Icon = "fas fa-tv" },
                new RoomFeature { Name = "Két an toàn", Icon = "fas fa-lock" }
            }
        },
        new RoomDisplayViewModel
        {
            Id = 2,
            Name = "Phòng Suite Gia ?ình",
            Description = "Không gian r?ng rãi dành cho gia ?ình v?i hai phòng ng? và phòng khách riêng bi?t.",
            ImageUrl = "/images/rooms/family-suite.jpg",
            RoomType = "Suite",
            BedType = "1 Gi??ng King & 2 Gi??ng ??n",
            Size = 80,
            Capacity = 4,
            PricePerNight = 3500000,
            OriginalPrice = 3500000,
            IsPromotion = false,
            AverageRating = 4.9,
            ReviewsCount = 98,
            IsFavorited = true,
            IncludeBreakfast = true,
            Features = new List<RoomFeature>
            {
                new RoomFeature { Name = "WiFi mi?n phí", Icon = "fas fa-wifi" },
                new RoomFeature { Name = "?i?u hòa", Icon = "fas fa-snowflake" },
                new RoomFeature { Name = "Minibar", Icon = "fas fa-glass-martini" },
                new RoomFeature { Name = "TV màn hình ph?ng", Icon = "fas fa-tv" },
                new RoomFeature { Name = "Két an toàn", Icon = "fas fa-lock" },
                new RoomFeature { Name = "B?n t?m spa", Icon = "fas fa-bath" }
            }
        },
        new RoomDisplayViewModel
        {
            Id = 3,
            Name = "Phòng Standard",
            Description = "Phòng tiêu chu?n tho?i mái v?i ??y ?? ti?n nghi c? b?n cho k? ngh? c?a b?n.",
            ImageUrl = "/images/rooms/standard-room.jpg",
            RoomType = "Standard",
            BedType = "2 Gi??ng ??n",
            Size = 30,
            Capacity = 2,
            PricePerNight = 1200000,
            OriginalPrice = 1200000,
            IsPromotion = false,
            AverageRating = 4.2,
            ReviewsCount = 56,
            IsFavorited = false,
            IncludeBreakfast = false,
            Features = new List<RoomFeature>
            {
                new RoomFeature { Name = "WiFi mi?n phí", Icon = "fas fa-wifi" },
                new RoomFeature { Name = "?i?u hòa", Icon = "fas fa-snowflake" },
                new RoomFeature { Name = "TV màn hình ph?ng", Icon = "fas fa-tv" }
            }
        },
        new RoomDisplayViewModel
        {
            Id = 4,
            Name = "Phòng Executive Business",
            Description = "Phòng thi?t k? dành cho doanh nhân v?i không gian làm vi?c riêng và ti?n nghi cao c?p.",
            ImageUrl = "/images/rooms/executive-room.jpg",
            RoomType = "Executive",
            BedType = "1 Gi??ng King",
            Size = 40,
            Capacity = 2,
            PricePerNight = 2200000,
            OriginalPrice = 2400000,
            IsPromotion = true,
            AverageRating = 4.7,
            ReviewsCount = 72,
            IsFavorited = false,
            IncludeBreakfast = true,
            Features = new List<RoomFeature>
            {
                new RoomFeature { Name = "WiFi t?c ?? cao", Icon = "fas fa-wifi" },
                new RoomFeature { Name = "?i?u hòa", Icon = "fas fa-snowflake" },
                new RoomFeature { Name = "Minibar", Icon = "fas fa-glass-martini" },
                new RoomFeature { Name = "TV màn hình ph?ng", Icon = "fas fa-tv" },
                new RoomFeature { Name = "Két an toàn", Icon = "fas fa-lock" },
                new RoomFeature { Name = "Bàn làm vi?c", Icon = "fas fa-desk" },
                new RoomFeature { Name = "Máy pha cà phê", Icon = "fas fa-coffee" }
            }
        },
        new RoomDisplayViewModel
        {
            Id = 5,
            Name = "Phòng T?ng Th?ng",
            Description = "Phòng sang tr?ng b?c nh?t v?i thi?t k? ??c ?áo, không gian r?ng rãi và d?ch v? riêng bi?t.",
            ImageUrl = "/images/rooms/presidential-suite.jpg",
            RoomType = "Presidential",
            BedType = "1 Gi??ng King size",
            Size = 120,
            Capacity = 2,
            PricePerNight = 8000000,
            OriginalPrice = 8000000,
            IsPromotion = false,
            AverageRating = 5.0,
            ReviewsCount = 45,
            IsFavorited = false,
            IncludeBreakfast = true,
            Features = new List<RoomFeature>
            {
                new RoomFeature { Name = "WiFi t?c ?? cao", Icon = "fas fa-wifi" },
                new RoomFeature { Name = "?i?u hòa", Icon = "fas fa-snowflake" },
                new RoomFeature { Name = "Minibar", Icon = "fas fa-glass-martini" },
                new RoomFeature { Name = "TV màn hình ph?ng", Icon = "fas fa-tv" },
                new RoomFeature { Name = "Két an toàn", Icon = "fas fa-lock" },
                new RoomFeature { Name = "Phòng khách riêng", Icon = "fas fa-couch" },
                new RoomFeature { Name = "B?n t?m Jacuzzi", Icon = "fas fa-hot-tub" },
                new RoomFeature { Name = "D?ch v? qu?n gia", Icon = "fas fa-concierge-bell" }
            }
        }
    };
        }
    }
}
