using HotelBookingSystem.Models;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HotelBookingSystem.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            // Trả về giao diện Dashboard
            return View("Dashboard");
        }

        [HttpGet("Bookings")]
        public IActionResult Bookings()
        {
            // Trả về giao diện quản lý đặt phòng
            return View("AdminBookings");
        }

        [HttpGet("Rooms")]
        public IActionResult Rooms()
        {
            // Để xem giao diện mà không cần dữ liệu từ DB
            var viewModel = new RoomsViewModel
            {
                AvailableCount = 32,
                OccupiedCount = 18,
                MaintenanceCount = 2,
                CleaningCount = 5,
                TotalRooms = 57,
                Rooms = GetSampleRooms()
            };

            return View("AdminRooms", viewModel);
        }

        private List<RoomItemViewModel> GetSampleRooms()
        {
            return new List<RoomItemViewModel>
    {
        new RoomItemViewModel
        {
            Id = 1,
            Name = "Phòng Deluxe Hướng Biển",
            RoomType = "Deluxe",
            PricePerNight = 2000000,
            Capacity = 2,
            Status = "Trống",
            Floor = 3,
            RoomNumber = "301",
            Rating = 4.8,
            ImageUrl = "/images/rooms/deluxe-ocean.jpg",
            Description = "Phòng sang trọng với tầm nhìn ra biển tuyệt đẹp",
            Amenities = new List<string> { "Wi-Fi", "TV", "Điều hòa", "Minibar", "Két an toàn" }
        },
        // Thêm các phòng mẫu khác tương tự
    };
        }

        [HttpGet("Users")]
        public IActionResult Users()
        {
            // Trả về giao diện quản lý khách hàng với dữ liệu mẫu
            var viewModel = new UsersViewModel
            {
                ActiveCount = 458,
                NewCount = 48,
                VipCount = 23,
                InactiveCount = 7,
                TotalUsers = 536,
                Users = GetSampleUsers()
            };

            return View("AdminUsers", viewModel);
        }

        private List<UserItemViewModel> GetSampleUsers()
        {
            return new List<UserItemViewModel>
    {
        new UserItemViewModel
        {
            Id = "1",
            UserId = "#UID001",
            FirstName = "Phạm",
            LastName = "Tuấn",
            Email = "phamtuan@mail.com",
            Phone = "0912345678",
            RegisterDate = DateTime.ParseExact("10/05/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 8,
            TotalSpending = 64500000,
            Status = "active",
            UserType = "vip",
            Address = "123 Nguyễn Văn Linh, Quận 7, TP. Hồ Chí Minh",
            LoyaltyPoints = 1450,
            LastLoginDate = DateTime.Now.AddHours(-2)
        },
        new UserItemViewModel
        {
            Id = "2",
            UserId = "#UID002",
            FirstName = "Nguyễn",
            LastName = "Hương",
            Email = "huongnguyen@mail.com",
            Phone = "0976543210",
            RegisterDate = DateTime.ParseExact("15/06/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 6,
            TotalSpending = 52800000,
            Status = "active",
            UserType = "vip",
            Address = "456 Lê Lợi, Quận 1, TP. Hồ Chí Minh",
            LoyaltyPoints = 1280,
            LastLoginDate = DateTime.Now.AddDays(-1)
        },
        new UserItemViewModel
        {
            Id = "3",
            UserId = "#UID003",
            FirstName = "Trần",
            LastName = "Linh",
            Email = "tranlinh@mail.com",
            Phone = "0932145678",
            RegisterDate = DateTime.ParseExact("22/07/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 5,
            TotalSpending = 48200000,
            Status = "active",
            UserType = "vip",
            Address = "789 Điện Biên Phủ, Quận 3, TP. Hồ Chí Minh",
            LoyaltyPoints = 950,
            LastLoginDate = DateTime.Now.AddDays(-3)
        },
        new UserItemViewModel
        {
            Id = "4",
            UserId = "#UID004",
            FirstName = "Lê",
            LastName = "Hoàng",
            Email = "lehoang@mail.com",
            Phone = "0987654321",
            RegisterDate = DateTime.ParseExact("05/09/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 2,
            TotalSpending = 12500000,
            Status = "inactive",
            UserType = "normal",
            Address = "101 Võ Văn Ngân, Thủ Đức, TP. Hồ Chí Minh",
            LoyaltyPoints = 250,
            LastLoginDate = DateTime.Now.AddDays(-10)
        },
        new UserItemViewModel
        {
            Id = "5",
            UserId = "#UID005",
            FirstName = "Vũ",
            LastName = "Minh",
            Email = "vuminh@mail.com",
            Phone = "0901234567",
            RegisterDate = DateTime.ParseExact("18/10/2023", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            BookingsCount = 1,
            TotalSpending = 3200000,
            Status = "pending",
            UserType = "normal",
            Address = "202 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh",
            LoyaltyPoints = 80,
            LastLoginDate = DateTime.Now.AddDays(-5)
        }
    };
        }

        [HttpGet("Reviews")]
        public IActionResult Reviews()
        {
            // Trả về giao diện quản lý đánh giá
            return View("AdminReviews");
        }

        [HttpGet("Promotions")]
        public IActionResult Promotions()
        {
            // Trả về giao diện quản lý khuyến mãi
            return View("AdminPromotions");
        }

        [HttpGet("Reports")]
        public IActionResult Reports()
        {
            // Trả về giao diện báo cáo tổng hợp
            return View("AdminReports");
        }

        [HttpGet("Settings")]
        public IActionResult Settings()
        {
            // Trả về giao diện cài đặt admin
            return View("AdminSettings");
        }

        [HttpGet("AddRoom")]
        public IActionResult AddRoom()
        {
            return View();
        }

        [HttpPost("AddRoom")]
        public async Task<IActionResult> AddRoom(RoomCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh (giả lập)
                    string mainImagePath = "/images/rooms/room-placeholder.jpg";
                    if (model.MainImage != null)
                    {
                        // Giả lập đường dẫn ảnh thành công
                        mainImagePath = $"/images/rooms/{Guid.NewGuid()}.jpg";
                    }

                    // Giả lập thành công và chuyển hướng
                    TempData["SuccessMessage"] = "Thêm phòng mới thành công!";
                    return RedirectToAction("Rooms");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi khi thêm phòng: {ex.Message}");
                }
            }

            return View(model);
        }
        [HttpGet("ExportRooms")]
        public async Task<IActionResult> ExportRooms(string format, [FromQuery] RoomsQueryViewModel query)
        {
            // Lấy lại dữ liệu phòng giống như màn Rooms
            var result = await _adminRoomService.GetRooms(query);
            var rooms = result.Rooms;

            if (!rooms.Any())
                return Content("Không có dữ liệu để xuất.");

            if (format == "excel")
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Rooms");

                // Header
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Tên phòng";
                worksheet.Cell(1, 3).Value = "Loại";
                worksheet.Cell(1, 4).Value = "Giá/đêm";
                worksheet.Cell(1, 5).Value = "Sức chứa";
                worksheet.Cell(1, 6).Value = "Trạng thái";

                // Dữ liệu
                int row = 2;
                foreach (var r in rooms)
                {
                    worksheet.Cell(row, 1).Value = r.Id;
                    worksheet.Cell(row, 2).Value = r.Name;
                    worksheet.Cell(row, 3).Value = r.RoomType;
                    worksheet.Cell(row, 4).Value = r.PricePerNight;
                    worksheet.Cell(row, 5).Value = r.Capacity;
                    worksheet.Cell(row, 6).Value = r.IsActivated ? "Hoạt động" : "Ngừng hoạt động";
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Rooms.xlsx");
            }
            else if (format == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Tên phòng,Loại,Giá/đêm,Sức chứa,Trạng thái");

                foreach (var r in rooms)
                {
                    csv.AppendLine($"{r.Id},{r.Name},{r.RoomType},{r.PricePerNight},{r.Capacity},{(r.IsActivated ? "Hoạt động" : "Ngừng hoạt động")}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", "Rooms.csv");
            }
            else if (format == "pdf")
            {
                using var stream = new MemoryStream();
                var writerProperties = new iText.Kernel.Pdf.WriterProperties();
                // KHÔNG dùng SmartMode ở đây
                var writer = new iText.Kernel.Pdf.PdfWriter(stream, writerProperties);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                document.Add(new iText.Layout.Element.Paragraph("Danh sách phòng"));

                var table = new iText.Layout.Element.Table(6, false);
                table.AddHeaderCell("ID");
                table.AddHeaderCell("Tên phòng");
                table.AddHeaderCell("Loại");
                table.AddHeaderCell("Giá/đêm");
                table.AddHeaderCell("Sức chứa");
                table.AddHeaderCell("Trạng thái");

                foreach (var r in rooms)
                {
                    table.AddCell(r.Id.ToString());
                    table.AddCell(r.Name);
                    table.AddCell(r.RoomType);
                    table.AddCell(r.PricePerNight.ToString("N0"));
                    table.AddCell(r.Capacity.ToString());
                    table.AddCell(r.IsActivated ? "Hoạt động" : "Ngừng hoạt động");
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "Rooms.pdf");
            }

            return BadRequest("Định dạng không hợp lệ (chỉ hỗ trợ excel, csv, pdf).");
        }
        [HttpGet("ExportUsers")]
        public async Task<IActionResult> ExportUsers(string format, [FromQuery] UserQueryOptions query)
        {
            var result = await _adminUserService.GetUsers(query);
            var users = result.Users;

            if (!users.Any())
                return Content("Không có dữ liệu để xuất.");

            if (format == "excel")
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Users");

                // Header
                worksheet.Cell(1, 1).Value = "Tên khách hàng";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "Số điện thoại";
                worksheet.Cell(1, 4).Value = "Ngày đăng ký";
                worksheet.Cell(1, 5).Value = "Số lần đặt phòng";
                worksheet.Cell(1, 6).Value = "Tổng chi tiêu";
                worksheet.Cell(1, 7).Value = "Email xác nhận";
                worksheet.Cell(1, 8).Value = "Trạng thái";

                // Dữ liệu
                int row = 2;
                foreach (var u in users)
                {
                    worksheet.Cell(row, 1).Value = u.FullName;
                    worksheet.Cell(row, 2).Value = u.Email;
                    worksheet.Cell(row, 3).Value = u.PhoneNumber;
                    worksheet.Cell(row, 4).Value = u.RegisteredDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 5).Value = u.BookingCount;
                    worksheet.Cell(row, 6).Value = u.TotalSpent;
                    worksheet.Cell(row, 7).Value = u.EmailConfirmed ? "Đã xác nhận" : "Chưa xác nhận";
                    worksheet.Cell(row, 8).Value = u.IsActivated ? "Hoạt động" : "Bị khóa";
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Users.xlsx");
            }
            else if (format == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Tên khách hàng,Email,Số điện thoại,Ngày đăng ký,Số lần đặt phòng,Tổng chi tiêu,Email xác nhận,Trạng thái");

                foreach (var u in users)
                {
                    csv.AppendLine($"{u.FullName},{u.Email},{u.PhoneNumber},{u.RegisteredDate:dd/MM/yyyy},{u.BookingCount},{u.TotalSpent},{(u.EmailConfirmed ? "Đã xác nhận" : "Chưa xác nhận")},{(u.IsActivated ? "Hoạt động" : "Bị khóa")}");
                }

                return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Users.csv");
            }
            else if (format == "pdf")
            {
                using var stream = new MemoryStream();
                var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                document.Add(new iText.Layout.Element.Paragraph("Danh sách khách hàng"));

                var table = new iText.Layout.Element.Table(8, false);
                table.AddHeaderCell("Tên khách hàng");
                table.AddHeaderCell("Email");
                table.AddHeaderCell("Số điện thoại");
                table.AddHeaderCell("Ngày đăng ký");
                table.AddHeaderCell("Đặt phòng");
                table.AddHeaderCell("Tổng chi tiêu");
                table.AddHeaderCell("Email xác nhận");
                table.AddHeaderCell("Trạng thái");

                foreach (var u in users)
                {
                    table.AddCell(u.FullName);
                    table.AddCell(u.Email);
                    table.AddCell(u.PhoneNumber);
                    table.AddCell(u.RegisteredDate.ToString("dd/MM/yyyy"));
                    table.AddCell(u.BookingCount.ToString());
                    table.AddCell(u.TotalSpent.ToString("N0") + " VNĐ");
                    table.AddCell(u.EmailConfirmed ? "Đã xác nhận" : "Chưa xác nhận");
                    table.AddCell(u.IsActivated ? "Hoạt động" : "Bị khóa");
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "Users.pdf");
            }

            return BadRequest("Định dạng không hợp lệ (excel, csv, pdf).");
        }
        [HttpGet("ExportReviews")]
        public async Task<IActionResult> ExportReviews(string format, [FromQuery] ReviewsQueryViewModel query)
        {
            var model = await _adminReviewService.GetReviews(query);
            var reviews = model.Reviews;

            if (!reviews.Any())
                return Content("Không có dữ liệu để xuất.");

            if (format == "excel")
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Reviews");

                worksheet.Cell(1, 1).Value = "Tên phòng";
                worksheet.Cell(1, 2).Value = "Tên khách hàng";
                worksheet.Cell(1, 3).Value = "Đánh giá";
                worksheet.Cell(1, 4).Value = "Nội dung";
                worksheet.Cell(1, 5).Value = "Ngày tạo";

                int row = 2;
                foreach (var r in reviews)
                {
                    worksheet.Cell(row, 1).Value = r.Room.Name;
                    worksheet.Cell(row, 2).Value = r.User.FullName;
                    worksheet.Cell(row, 3).Value = r.Rating;
                    worksheet.Cell(row, 4).Value = r.Comment;
                    worksheet.Cell(row, 5).Value = r.CreatedDate.ToString("dd/MM/yyyy");
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Reviews.xlsx");
            }
            else if (format == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Tên phòng,Tên khách hàng,Đánh giá,Nội dung,Ngày tạo");

                foreach (var r in reviews)
                {
                    csv.AppendLine($"{r.Room.Name},{r.User.FullName},{r.Rating},{r.Comment},{r.CreatedDate:dd/MM/yyyy}");
                }

                return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Reviews.csv");
            }
            else if (format == "pdf")
            {
                using var stream = new MemoryStream();
                var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                document.Add(new iText.Layout.Element.Paragraph("Danh sách đánh giá"));

                var table = new iText.Layout.Element.Table(5, false);
                table.AddHeaderCell("Tên phòng");
                table.AddHeaderCell("Tên khách hàng");
                table.AddHeaderCell("Đánh giá");
                table.AddHeaderCell("Nội dung");
                table.AddHeaderCell("Ngày tạo");

                foreach (var r in reviews)
                {
                    table.AddCell(r.Room.Name);
                    table.AddCell(r.User.FullName);
                    table.AddCell(r.Rating.ToString());
                    table.AddCell(r.Comment);
                    table.AddCell(r.CreatedDate.ToString("dd/MM/yyyy"));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "Reviews.pdf");
            }

            return BadRequest("Định dạng không hợp lệ (excel, csv, pdf).");
        }

    }
}
