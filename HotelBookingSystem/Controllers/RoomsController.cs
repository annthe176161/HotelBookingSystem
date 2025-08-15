using HotelBookingSystem.Data;
using HotelBookingSystem.ViewModels.RoomsViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return View(rooms);
        }

        public IActionResult Details(int id)
        {
            // Trong thực tế, dữ liệu này sẽ được lấy từ database
            var roomDetails = GetSampleRoomDetails(id);

            if (roomDetails == null)
            {
                return NotFound();
            }

            return View(roomDetails);
        }

        private RoomDetailsViewModel GetSampleRoomDetails(int id)
        {
            // Sample room data for display purposes
            var roomDetails = new RoomDetailsViewModel
            {
                Id = id,
                Name = id == 1 ? "Phòng Deluxe Hướng Biển" :
                      id == 2 ? "Phòng Suite Gia Đình" :
                      id == 3 ? "Phòng Standard" :
                      id == 4 ? "Phòng Executive Business" : "Phòng Tổng Thống",
                Description = id == 1 ? "Phòng rộng rãi với view biển tuyệt đẹp, được trang bị đầy đủ tiện nghi hiện đại. Phòng được thiết kế theo phong cách hiện đại, thanh lịch với những gam màu trung tính, tạo cảm giác thư giãn và gần gũi với thiên nhiên.<br><br>Phòng có ban công riêng, nơi bạn có thể ngắm nhìn biển cả mênh mông và thưởng thức những khoảnh khắc bình minh tuyệt đẹp. Không gian phòng tràn ngập ánh sáng tự nhiên, kết hợp với nội thất gỗ sang trọng, tạo nên không gian nghỉ dưỡng đẳng cấp." :
                       id == 2 ? "Không gian rộng rãi dành cho gia đình với hai phòng ngủ và phòng khách riêng biệt. Phòng Suite Gia Đình là lựa chọn hoàn hảo cho các gia đình đang tìm kiếm không gian nghỉ dưỡng thoải mái và tiện nghi.<br><br>Phòng có hai phòng ngủ riêng biệt, một phòng với giường King size và một phòng với hai giường đơn, đảm bảo không gian riêng tư cho các thành viên trong gia đình. Khu vực phòng khách rộng rãi với sofa có thể chuyển thành giường, TV màn hình phẳng và bàn làm việc." :
                       id == 3 ? "Phòng tiêu chuẩn thoải mái với đầy đủ tiện nghi cơ bản cho kỳ nghỉ của bạn. Mặc dù là phòng tiêu chuẩn nhưng vẫn được trang bị đầy đủ tiện nghi hiện đại và được thiết kế theo phong cách đơn giản, thanh lịch.<br><br>Phòng có hai giường đơn, phù hợp cho các cặp đôi hoặc du khách đi công tác. Cửa sổ lớn giúp phòng luôn tràn ngập ánh sáng tự nhiên. Phòng tắm riêng được trang bị đầy đủ các vật dụng cá nhân chất lượng cao." :
                       id == 4 ? "Phòng thiết kế dành cho doanh nhân với không gian làm việc riêng và tiện nghi cao cấp. Phòng Executive Business được thiết kế đặc biệt cho các doanh nhân, với không gian làm việc riêng biệt và các tiện nghi hiện đại phục vụ công việc.<br><br>Phòng có giường King size cùng với khu vực làm việc rộng rãi, trang bị bàn làm việc lớn, ghế ergonomic, cổng kết nối đa phương tiện và WiFi tốc độ cao. Phòng cũng có khu vực tiếp khách nhỏ, lý tưởng cho các cuộc họp không chính thức." :
                       "Phòng sang trọng bậc nhất với thiết kế độc đáo, không gian rộng rãi và dịch vụ riêng biệt. Phòng Tổng Thống là phòng cao cấp nhất của khách sạn, mang đến trải nghiệm lưu trú xa xỉ và đẳng cấp nhất.<br><br>Phòng có diện tích lớn với phòng khách riêng biệt, phòng ngủ với giường King size, phòng tắm xa hoa với bồn tắm Jacuzzi và vòi sen mưa nhiệt đới. Ban công rộng rãi nhìn ra biển hoặc thành phố. Khách lưu trú tại phòng này còn được hưởng dịch vụ quản gia riêng, check-in/check-out tại phòng và nhiều đặc quyền độc quyền khác.",
                MainImageUrl = id == 1 ? "/images/rooms/deluxe-ocean.jpg" :
                              id == 2 ? "/images/rooms/family-suite.jpg" :
                              id == 3 ? "/images/rooms/standard-room.jpg" :
                              id == 4 ? "/images/rooms/executive-room.jpg" : "/images/rooms/presidential-suite.jpg",
                ImageUrls = new List<string>
                {
                    id == 1 ? "/images/rooms/deluxe-ocean.jpg" : id == 2 ? "/images/rooms/family-suite.jpg" : id == 3 ? "/images/rooms/standard-room.jpg" : id == 4 ? "/images/rooms/executive-room.jpg" : "/images/rooms/presidential-suite.jpg",
                    "/images/rooms/room-detail-1.jpg",
                    "/images/rooms/room-detail-2.jpg",
                    "/images/rooms/room-detail-3.jpg",
                    "/images/rooms/room-detail-4.jpg"
                },
                RoomType = id == 1 ? "Deluxe" : id == 2 ? "Suite" : id == 3 ? "Standard" : id == 4 ? "Executive" : "Presidential",
                BedType = id == 1 ? "1 Giường King" : id == 2 ? "1 Giường King & 2 Giường Đơn" : id == 3 ? "2 Giường Đơn" : id == 4 ? "1 Giường King" : "1 Giường King size",
                Size = id == 1 ? 45 : id == 2 ? 80 : id == 3 ? 30 : id == 4 ? 40 : 120,
                Capacity = id == 1 ? 2 : id == 2 ? 4 : id == 3 ? 2 : id == 4 ? 2 : 2,
                PricePerNight = id == 1 ? 2000000 : id == 2 ? 3500000 : id == 3 ? 1200000 : id == 4 ? 2200000 : 8000000,
                OriginalPrice = id == 1 ? 2500000 : id == 2 ? 3500000 : id == 3 ? 1200000 : id == 4 ? 2400000 : 8000000,
                AverageRating = id == 1 ? 4.8 : id == 2 ? 4.9 : id == 3 ? 4.2 : id == 4 ? 4.7 : 5.0,
                ReviewsCount = id == 1 ? 124 : id == 2 ? 98 : id == 3 ? 56 : id == 4 ? 72 : 45,
                IsFavorited = id == 2, // Only room 2 is favorited (example)
                Floor = id == 1 ? "3" : id == 2 ? "5" : id == 3 ? "2" : id == 4 ? "7" : "10",
                Building = "Tòa chính",
                View = id == 1 ? "Biển" : id == 2 ? "Thành phố" : id == 3 ? "Vườn" : id == 4 ? "Thành phố" : "Biển panorama",
                IncludeBreakfast = id == 1 || id == 2 || id == 4 || id == 5,
                RatingDistribution = new Dictionary<int, int>
                {
                    { 5, id == 5 ? 100 : id == 1 || id == 2 ? 70 : id == 4 ? 60 : 40 },
                    { 4, id == 5 ? 0 : id == 1 || id == 2 ? 20 : id == 4 ? 30 : 35 },
                    { 3, id == 5 ? 0 : id == 1 || id == 2 ? 7 : id == 4 ? 7 : 20 },
                    { 2, id == 5 ? 0 : id == 1 || id == 2 ? 2 : id == 4 ? 2 : 3 },
                    { 1, id == 5 ? 0 : id == 1 || id == 2 ? 1 : id == 4 ? 1 : 2 }
                },
                Features = new List<RoomFeature>
                {
                    new RoomFeature { Name = "WiFi miễn phí", Icon = "fas fa-wifi" },
                    new RoomFeature { Name = "Điều hòa", Icon = "fas fa-snowflake" },
                    new RoomFeature { Name = "Minibar", Icon = "fas fa-glass-martini" },
                    new RoomFeature { Name = "TV màn hình phẳng", Icon = "fas fa-tv" },
                    new RoomFeature { Name = "Két an toàn", Icon = "fas fa-lock" },

                    id == 2 || id == 5 ? new RoomFeature { Name = "Bồn tắm spa", Icon = "fas fa-bath" } : null,
                    id == 4 || id == 5 ? new RoomFeature { Name = "Bàn làm việc", Icon = "fas fa-desk" } : null,
                    id == 4 || id == 5 ? new RoomFeature { Name = "Máy pha cà phê", Icon = "fas fa-coffee" } : null,
                    id == 5 ? new RoomFeature { Name = "Phòng khách riêng", Icon = "fas fa-couch" } : null,
                    id == 5 ? new RoomFeature { Name = "Bồn tắm Jacuzzi", Icon = "fas fa-hot-tub" } : null,
                    id == 5 ? new RoomFeature { Name = "Dịch vụ quản gia", Icon = "fas fa-concierge-bell" } : null
                }.Where(f => f != null).ToList(),

                Reviews = new List<ReviewViewModel>
                {
                    new ReviewViewModel
                    {
                        UserName = "Nguyễn Văn A",
                        UserAvatarUrl = "",
                        Rating = 5,
                        Date = DateTime.Now.AddDays(-5),
                        Comment = "Phòng rất sạch sẽ và thoải mái. Nhân viên phục vụ tận tình, nhiệt tình. Sẽ quay lại lần sau!"
                    },
                    new ReviewViewModel
                    {
                        UserName = "Trần Thị B",
                        UserAvatarUrl = "/images/avatars/avatar1.jpg",
                        Rating = 4,
                        Date = DateTime.Now.AddDays(-12),
                        Comment = "Vị trí tuyệt vời, view đẹp. Tuy nhiên phòng hơi ồn vào buổi sáng."
                    },
                    new ReviewViewModel
                    {
                        UserName = "Lê Văn C",
                        UserAvatarUrl = "",
                        Rating = 5,
                        Date = DateTime.Now.AddDays(-20),
                        Comment = "Một trong những khách sạn tốt nhất mà tôi từng ở. Đồ ăn sáng ngon, nhân viên nhiệt tình, phòng đẹp."
                    }
                },

                // Similar rooms suggestions
                SimilarRooms = new List<SimilarRoomViewModel>
                {
                    new SimilarRoomViewModel
                    {
                        Id = id == 1 ? 4 : 1,
                        Name = id == 1 ? "Phòng Executive Business" : "Phòng Deluxe Hướng Biển",
                        ImageUrl = id == 1 ? "/images/rooms/executive-room.jpg" : "/images/rooms/deluxe-ocean.jpg",
                        Price = id == 1 ? 2200000 : 2000000,
                        Rating = id == 1 ? 4.7 : 4.8,
                        Size = id == 1 ? 40 : 45,
                        Capacity = 2
                    },
                    new SimilarRoomViewModel
                    {
                        Id = id == 2 ? 5 : 2,
                        Name = id == 2 ? "Phòng Tổng Thống" : "Phòng Suite Gia Đình",
                        ImageUrl = id == 2 ? "/images/rooms/presidential-suite.jpg" : "/images/rooms/family-suite.jpg",
                        Price = id == 2 ? 8000000 : 3500000,
                        Rating = id == 2 ? 5.0 : 4.9,
                        Size = id == 2 ? 120 : 80,
                        Capacity = id == 2 ? 2 : 4
                    },
                    new SimilarRoomViewModel
                    {
                        Id = id == 3 ? 1 : 3,
                        Name = id == 3 ? "Phòng Deluxe Hướng Biển" : "Phòng Standard",
                        ImageUrl = id == 3 ? "/images/rooms/deluxe-ocean.jpg" : "/images/rooms/standard-room.jpg",
                        Price = id == 3 ? 2000000 : 1200000,
                        Rating = id == 3 ? 4.8 : 4.2,
                        Size = id == 3 ? 45 : 30,
                        Capacity = 2
                    }
                }
            };

            return roomDetails;
        }
    }
}
