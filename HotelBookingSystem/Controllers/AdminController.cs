using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Implementations;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HotelBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminRoomService _adminRoomService;
        private readonly IAdminUserService _adminUserService;
        private readonly IAdminBookingService _adminBookingService;
        private readonly IAdminReviewService _adminReviewService;
        private readonly IAdminDashboardService _dashboardService;

        public AdminController(
            IAdminRoomService adminRoomService, 
            IAdminUserService adminUserService, 
            IAdminBookingService adminBookingService, 
            IAdminReviewService adminReviewService,
            IAdminDashboardService dashboardService)
        {
            _adminRoomService = adminRoomService;
            _adminUserService = adminUserService;
            _adminBookingService = adminBookingService;
            _adminReviewService = adminReviewService;
            _dashboardService = dashboardService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            // Chuyển hướng đến Dashboard
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var dashboardData = await _dashboardService.GetDashboardData(startOfMonth, endOfMonth);
            return View(dashboardData);
        }

        [HttpGet("Rooms")]
        public async Task<IActionResult> Rooms([FromQuery] RoomsQueryViewModel query)
        {
            var result = await _adminRoomService.GetRooms(query);

            return View("AdminRooms", result);
        }

        [HttpPost("AddRoom")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(CreateRoomViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _adminRoomService.Add(model, ct);
                return RedirectToAction("Rooms");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi tạo phòng: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost("Deactivate/{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _adminRoomService.DeactivateRoom(id);
            return RedirectToAction("Rooms");
        }

        [HttpPost("Activate/{id}")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _adminRoomService.ActivateRoom(id);
            return RedirectToAction("Rooms");
        }

        [HttpGet("EditRoom/{id}")]
        public async Task<IActionResult> EditRoom(int id, CancellationToken ct)
        {
            var vm = await _adminRoomService.GetById(id, ct);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost("EditRoom/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, RoomDetailsViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updatedRoom = await _adminRoomService.Update(model.Room, ct);
                if (updatedRoom == null) return NotFound();

                return RedirectToAction("Rooms");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi cập nhật phòng: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet("AddUser")]
        public IActionResult AddUser()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost("AddUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(CreateUserViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _adminUserService.Add(model, ct);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            return RedirectToAction("Users");
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users([FromQuery] UserQueryOptions options)
        {
            var result = await _adminUserService.GetUsers(options);
            return View("AdminUsers", result);
        }

        [HttpGet("EditUser/{id}")]
        public async Task<IActionResult> EditUser(string id, CancellationToken ct)
        {
            var vm = await _adminUserService.GetById(id, ct);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost("EditUser/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserDetailsViewModel wrapper, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(wrapper);

            var result = await _adminUserService.Update(wrapper.User, ct);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(wrapper);
            }

            return RedirectToAction("Users");
        }

        [HttpGet("Bookings")]
        public async Task<IActionResult> Bookings([FromQuery] BookingQueryOptions query)
        {
            var model = await _adminBookingService.GetBookings(query);
            return View("AdminBookings", model);
        }

        [HttpPost]
        [Route("Admin/UpdateBookingStatus")]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var result = await _adminBookingService.UpdateBookingStatus(id, status);

            if (!result.success)
            {
                TempData["Error"] = result.message;
            }
            else
            {
                TempData["Success"] = result.message;
            }

            // Always redirect back to Bookings page after action
            return RedirectToAction("Bookings");
        }

        [HttpPost]
        [Route("Admin/UpdatePaymentStatus")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, string paymentStatus)
        {
            var result = await _adminBookingService.UpdatePaymentStatus(id, paymentStatus);

            if (!result.success)
            {
                TempData["Error"] = result.message;
            }
            else
            {
                TempData["Success"] = result.message;
            }

            // Always redirect back to Bookings page after action
            return RedirectToAction("Bookings");
        }

        [HttpPost]
        [Route("Admin/CancelBooking")]
        public async Task<IActionResult> CancelBooking(int id, string cancelReason)
        {
            if (string.IsNullOrWhiteSpace(cancelReason))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy phòng.";
                return RedirectToAction("Bookings");
            }

            var result = await _adminBookingService.CancelBookingWithReason(id, cancelReason);

            if (!result.success)
            {
                TempData["Error"] = result.message;
            }
            else
            {
                TempData["Success"] = result.message;
            }

            // Always redirect back to Bookings page after action
            return RedirectToAction("Bookings");
        }

        [HttpGet("BookingDetails/{id}")]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var bookingDetails = await _adminBookingService.GetBookingDetails(id);

            if (bookingDetails == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đặt phòng.";
                return RedirectToAction("Bookings");
            }

            return View("AdminBookingDetails", bookingDetails);
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

        [HttpGet("Reviews")]
        public async Task<IActionResult> Reviews([FromQuery] ReviewsQueryViewModel query)
        {
            var result = await _adminReviewService.GetReviews(query);

            return View("AdminReviews", result);
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
