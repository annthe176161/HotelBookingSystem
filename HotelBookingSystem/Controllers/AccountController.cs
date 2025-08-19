using HotelBookingSystem.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Profile()
        {
            // Tạo dữ liệu mẫu cho trang profile
            var model = new ProfileViewModel
            {
                FirstName = "Nguyễn",
                LastName = "Văn A",
                Email = "nguyenvana@gmail.com",
                PhoneNumber = "0987654321",
                Birthdate = new DateTime(1990, 1, 1),
                Gender = "male",
                Address = "123 Phố Hàng Bài",
                City = "Hà Nội",
                State = "Hà Nội",
                ZipCode = "10000",
                CreatedAt = DateTime.Now.AddMonths(-6),
                BookingsCount = 5,
                ReviewsCount = 3,
                LoyaltyPoints = 250,
                LoyaltyTier = "Silver",
                NextTier = "Gold",
                PointsToNextTier = 150,
                NextTierProgress = 62,
                TwoFactorEnabled = false,

                // Thêm dữ liệu đặt phòng mẫu
                Bookings = new List<BookingItemViewModel>
                {
                    new BookingItemViewModel
                    {
                        Id = 1,
                        BookingNumber = "BK001",
                        RoomName = "Phòng Deluxe Hướng Biển",
                        RoomImageUrl = "/images/rooms/deluxe-ocean.jpg",
                        CheckInDate = DateTime.Now.AddDays(14),
                        CheckOutDate = DateTime.Now.AddDays(17),
                        GuestsCount = 2,
                        TotalPrice = 6000000,
                        Status = "Confirmed",
                        BookingDate = DateTime.Now.AddDays(-10),
                        HasReview = false
                    },
                    new BookingItemViewModel
                    {
                        Id = 2,
                        BookingNumber = "BK002",
                        RoomName = "Phòng Suite Gia Đình",
                        RoomImageUrl = "/images/rooms/family-suite.jpg",
                        CheckInDate = DateTime.Now.AddDays(-14),
                        CheckOutDate = DateTime.Now.AddDays(-10),
                        GuestsCount = 4,
                        TotalPrice = 14000000,
                        Status = "Completed",
                        BookingDate = DateTime.Now.AddDays(-25),
                        HasReview = true
                    }
                },

                // Thêm đánh giá mẫu
                Reviews = new List<ReviewItemViewModel>
                {
                    new ReviewItemViewModel
                    {
                        Id = 1,
                        RoomName = "Phòng Suite Gia Đình",
                        Rating = 5,
                        Comment = "Phòng rất tuyệt vời, rộng rãi và sạch sẽ. Nhân viên thân thiện và nhiệt tình. Sẽ quay lại lần sau!",
                        CreatedAt = DateTime.Now.AddDays(-9)
                    },
                    new ReviewItemViewModel
                    {
                        Id = 2,
                        RoomName = "Phòng Executive Business",
                        Rating = 4,
                        Comment = "Phòng tốt, đầy đủ tiện nghi cho chuyến công tác. Chỉ tiếc là wifi hơi chậm.",
                        CreatedAt = DateTime.Now.AddDays(-45)
                    }
                },

                // Thêm phòng yêu thích mẫu
                FavoriteRooms = new List<FavoriteRoomViewModel>
                {
                    new FavoriteRoomViewModel
                    {
                        Id = 1,
                        Name = "Phòng Tổng Thống",
                        ImageUrl = "/images/rooms/presidential-suite.jpg",
                        Price = 8000000,
                        Rating = 5.0,
                        Capacity = 2,
                        RoomType = "Presidential"
                    },
                    new FavoriteRoomViewModel
                    {
                        Id = 2,
                        Name = "Phòng Deluxe Hướng Biển",
                        ImageUrl = "/images/rooms/deluxe-ocean.jpg",
                        Price = 2000000,
                        Rating = 4.5,
                        Capacity = 2,
                        RoomType = "Deluxe"
                    }
                },

                // Thêm lịch sử điểm thưởng mẫu
                LoyaltyPointsHistory = new List<LoyaltyPointHistoryViewModel>
                {
                    new LoyaltyPointHistoryViewModel
                    {
                        Date = DateTime.Now.AddDays(-10),
                        Description = "Đặt phòng BK001",
                        Points = 60,
                        Status = "Completed"
                    },
                    new LoyaltyPointHistoryViewModel
                    {
                        Date = DateTime.Now.AddDays(-25),
                        Description = "Đặt phòng BK002",
                        Points = 140,
                        Status = "Completed"
                    },
                    new LoyaltyPointHistoryViewModel
                    {
                        Date = DateTime.Now.AddDays(-60),
                        Description = "Điểm thưởng chào mừng",
                        Points = 50,
                        Status = "Completed"
                    }
                },

                // Thêm hoạt động đăng nhập mẫu
                LoginActivities = new List<LoginActivityViewModel>
                {
                    new LoginActivityViewModel
                    {
                        Device = "Windows Chrome 115.0.5790",
                        Location = "Hà Nội, Việt Nam",
                        IpAddress = "203.113.148.XX",
                        Time = DateTime.Now.AddHours(-1),
                        Status = "Current"
                    },
                    new LoginActivityViewModel
                    {
                        Device = "iPhone Safari iOS 16.5",
                        Location = "Hà Nội, Việt Nam",
                        IpAddress = "42.116.83.XX",
                        Time = DateTime.Now.AddDays(-2),
                        Status = "Success"
                    },
                    new LoginActivityViewModel
                    {
                        Device = "Android Chrome 115.0.5790",
                        Location = "Hà Nội, Việt Nam",
                        IpAddress = "14.241.224.XX",
                        Time = DateTime.Now.AddDays(-7),
                        Status = "Success"
                    }
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Xử lý đăng xuất
            return RedirectToAction("Index", "Home");
        }
    }
}
