using HotelBookingSystem.Data;
using HotelBookingSystem.Services.Interfaces;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public interface IAdminBookingService
    {
        Task<BookingsViewModel> GetBookings(BookingQueryOptions query);
        Task<(bool success, string message)> UpdateBookingStatus(int bookingId, string newStatus);
        Task<(bool success, string message)> CancelBookingWithReason(int bookingId, string cancelReason);
        Task<(bool success, string message)> UpdatePaymentStatus(int bookingId, string newPaymentStatus);
        Task<AdminBookingDetailsViewModel?> GetBookingDetails(int bookingId);
    }

    public class AdminBookingService : IAdminBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AdminBookingService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<BookingsViewModel> GetBookings(BookingQueryOptions query)
        {
            var queryable = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .AsQueryable();

            if (query.BookingId.HasValue)
                queryable = queryable.Where(b => b.Id == query.BookingId.Value);

            if (!string.IsNullOrEmpty(query.CustomerName))
                queryable = queryable.Where(b => b.User!.FullName!.Contains(query.CustomerName));

            if (!string.IsNullOrEmpty(query.Status))
                queryable = queryable.Where(b => b.BookingStatus.Name == query.Status);

            if (query.StartDate.HasValue)
                queryable = queryable.Where(b => b.CheckIn >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(b => b.CheckOut <= query.EndDate.Value);

            // Total count before paging
            var totalBookings = await queryable.CountAsync();

            // Paging
            var bookings = await queryable
                .OrderByDescending(b => b.CreatedDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BookingListItemViewModel
                {
                    Id = b.Id,
                    CustomerName = b.User.FullName ?? "",
                    CustomerEmail = b.User.Email ?? "",
                    RoomName = b.Room.Name,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Guests = b.Guests,
                    TotalPrice = b.TotalPrice,
                    BookingStatus = b.BookingStatus.Name,
                    PaymentStatus = b.Payment != null ? b.Payment.PaymentStatus.Name : "Đang xử lý"
                })
                .ToListAsync();

            var pending = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Chờ xác nhận");
            var confirmed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Đã xác nhận");
            var completed = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Hoàn thành");
            var cancelled = await _context.Bookings.CountAsync(b => b.BookingStatus.Name == "Đã hủy");

            return new BookingsViewModel
            {
                Bookings = bookings,
                CurrentPage = query.Page,
                TotalPages = (int)Math.Ceiling(totalBookings / (double)query.PageSize),
                TotalBookings = totalBookings,
                Query = query,
                Pending = pending,
                Confirmed = confirmed,
                Completed = completed,
                Cancelled = cancelled
            };
        }

        public async Task<(bool success, string message)> UpdateBookingStatus(int bookingId, string newStatus)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy đặt phòng.");

            // Prevent invalid updates (already completed or cancelled)
            if (booking.BookingStatus.Name == "Hoàn thành" || booking.BookingStatus.Name == "Đã hủy")
                return (false, "Không thể cập nhật đặt phòng đã hoàn thành hoặc đã hủy.");

            var oldStatus = booking.BookingStatus.Name;

            // Kiểm tra logic business khi hoàn thành booking
            if (newStatus == "Hoàn thành")
            {
                // Logic mới: Hoàn thành có nghĩa là khách đã check-out và thanh toán
                // Tự động cập nhật payment status thành "Thành công" khi hoàn thành
                if (booking.Payment != null && booking.Payment.PaymentStatus.Name == "Đang xử lý")
                {
                    var successStatus = await _context.PaymentStatuses.FirstOrDefaultAsync(s => s.Name == "Thành công");
                    if (successStatus != null)
                    {
                        booking.Payment.PaymentStatus = successStatus;
                        _context.Update(booking.Payment);
                    }
                }
            }

            // Xử lý hủy phòng - Logic mới: Chỉ thanh toán tại khách sạn
            // Khi hủy phòng, payment status giữ nguyên "Đang xử lý" (vì chưa thanh toán)
            // Không cần logic hoàn tiền vì khách chưa thanh toán

            // Set new status
            var newBookingStatus = await _context.BookingStatuses.FirstOrDefaultAsync(s => s.Name == newStatus);
            if (newBookingStatus != null)
            {
                booking.BookingStatus = newBookingStatus;

                // Cập nhật ngày hoàn thành khi trạng thái chuyển thành "Hoàn thành"
                if (newStatus == "Hoàn thành")
                {
                    booking.CompletedDate = DateTime.Now;
                }

                // Cập nhật trạng thái phòng dựa trên trạng thái booking
                if (booking.Room != null)
                {
                    if (newStatus == "Hoàn thành" || newStatus == "Đã hủy")
                    {
                        // Phòng trở lại khả dụng khi booking hoàn thành hoặc bị hủy
                        booking.Room.IsAvailable = true;
                    }
                    else if (newStatus == "Đã xác nhận")
                    {
                        // Đảm bảo phòng không khả dụng khi booking được xác nhận
                        booking.Room.IsAvailable = false;
                    }
                    _context.Rooms.Update(booking.Room);
                }
            }

            if (booking.BookingStatus == null)
                return (false, "Không tìm thấy trạng thái mới.");

            _context.Update(booking);
            await _context.SaveChangesAsync();

            string successMessage = newStatus switch
            {
                "Đã hủy" => booking.Payment?.PaymentStatus?.Name == "Đã hoàn tiền"
                    ? "Đã hủy đặt phòng và hoàn tiền thành công."
                    : "Đã hủy đặt phòng thành công.",
                "Hoàn thành" => "Đặt phòng đã được hoàn thành thành công.",
                "Đã xác nhận" => "Đã xác nhận đặt phòng thành công.",
                _ => "Cập nhật trạng thái thành công."
            };

            // Gửi thông báo real-time cho khách hàng
            if (booking.User != null)
            {
                var customerMessage = newStatus switch
                {
                    "Đã xác nhận" => $"Đặt phòng #{bookingId} của bạn tại {booking.Room?.Name} đã được xác nhận.",
                    "Hoàn thành" => $"Đặt phòng #{bookingId} của bạn tại {booking.Room?.Name} đã hoàn thành thành công.",
                    "Đã hủy" => $"Đặt phòng #{bookingId} của bạn tại {booking.Room?.Name} đã bị hủy.",
                    _ => $"Trạng thái đặt phòng #{bookingId} của bạn đã được cập nhật thành: {newStatus}"
                };

                await _notificationService.SendBookingStatusUpdateToCustomerAsync(
                    booking.User.Id,
                    bookingId.ToString(),
                    newStatus,
                    customerMessage
                );
            }

            return (true, successMessage);
        }

        public async Task<(bool success, string message)> UpdatePaymentStatus(int bookingId, string newPaymentStatus)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Payment == null)
                return (false, "Không tìm thấy đặt phòng hoặc thông tin thanh toán.");

            var oldPaymentStatus = booking.Payment.PaymentStatus.Name;

            // LOGIC MỚI: Chặn cập nhật payment thành "Thành công" nếu booking chưa hoàn thành
            if (newPaymentStatus == "Thành công")
            {
                if (booking.BookingStatus.Name == "Chờ xác nhận" || booking.BookingStatus.Name == "Đã xác nhận")
                {
                    return (false, "Không thể cập nhật thanh toán thành 'Thành công' khi booking chưa hoàn thành. Vui lòng hoàn thành đặt phòng trước.");
                }
            }

            // Chỉ cho phép 2 trạng thái: "Đang xử lý" và "Thành công"
            if (newPaymentStatus != "Đang xử lý" && newPaymentStatus != "Thành công")
            {
                return (false, "Chỉ có thể cập nhật trạng thái thanh toán thành 'Đang xử lý' hoặc 'Thành công'.");
            }

            // Tìm payment status mới
            var newStatus = await _context.PaymentStatuses.FirstOrDefaultAsync(s => s.Name == newPaymentStatus);
            if (newStatus == null)
                return (false, $"Không tìm thấy trạng thái thanh toán '{newPaymentStatus}' trong hệ thống.");

            // Cập nhật payment status
            booking.Payment.PaymentStatus = newStatus;
            if (newPaymentStatus == "Thành công")
            {
                booking.Payment.PaymentDate = DateTime.Now;
            }

            _context.Update(booking.Payment);
            await _context.SaveChangesAsync();

            // Gửi thông báo real-time cho khách hàng về cập nhật thanh toán
            if (booking.User != null && oldPaymentStatus != newPaymentStatus)
            {
                var customerMessage = newPaymentStatus switch
                {
                    "Thành công" => $"Thanh toán cho đặt phòng #{bookingId} tại {booking.Room?.Name} đã được xác nhận thành công.",
                    "Đang xử lý" => $"Trạng thái thanh toán cho đặt phòng #{bookingId} tại {booking.Room?.Name} đang được xử lý.",
                    _ => $"Trạng thái thanh toán cho đặt phòng #{bookingId} đã được cập nhật."
                };

                await _notificationService.SendPaymentNotificationAsync(
                    booking.User.Id,
                    bookingId.ToString(),
                    newPaymentStatus,
                    customerMessage
                );
            }

            return (true, $"Cập nhật trạng thái thanh toán thành '{newPaymentStatus}' thành công.");
        }

        public async Task<AdminBookingDetailsViewModel?> GetBookingDetails(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return null;

            return new AdminBookingDetailsViewModel
            {
                // Booking Information
                Id = booking.Id,
                BookingNumber = $"#BK{booking.Id.ToString().PadLeft(6, '0')}",
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                Guests = booking.Guests,
                TotalPrice = booking.TotalPrice,
                CreatedDate = booking.CreatedDate,
                CompletedDate = booking.CompletedDate,
                Status = booking.BookingStatus.Name,
                SpecialRequests = "Không có yêu cầu đặc biệt", // Có thể thêm field này vào model sau

                // Customer Information
                CustomerName = booking.User.FullName ?? $"{booking.User.FirstName} {booking.User.LastName}".Trim(),
                CustomerEmail = booking.User.Email ?? "",
                CustomerPhone = booking.User.PhoneNumber ?? "Chưa cập nhật",
                CustomerJoinDate = DateTime.Now, // Sử dụng giá trị mặc định vì không có CreatedDate trong ApplicationUser

                // Room Information
                RoomId = booking.Room.Id,
                RoomName = booking.Room.Name,
                RoomType = booking.Room.RoomType,
                RoomDescription = booking.Room.Description ?? "Không có mô tả",
                RoomImageUrl = booking.Room.ImageUrl,
                RoomPricePerNight = booking.Room.PricePerNight,
                RoomCapacity = booking.Room.Capacity,
                RoomBedType = "Giường đôi", // Giá trị mặc định vì không có BedType trong Room model
                RoomFloor = "Tầng 1", // Giá trị mặc định vì không có Floor trong Room model
                RoomBuilding = "Tòa nhà chính", // Giá trị mặc định vì không có Building trong Room model

                // Payment Information
                PaymentId = booking.Payment?.Id,
                PaymentMethod = booking.Payment?.PaymentMethod ?? "Không xác định",
                PaymentStatus = booking.Payment?.PaymentStatus?.Name ?? "Đang xử lý",
                TransactionId = booking.Payment?.TransactionId ?? "",
                PaymentDate = booking.Payment?.PaymentDate,
                PaymentAmount = booking.Payment?.Amount ?? booking.TotalPrice,

                // Review Information
                HasReview = booking.Review != null,
                ReviewRating = booking.Review?.Rating,
                ReviewComment = booking.Review?.Comment ?? "",
                ReviewDate = booking.Review?.CreatedDate
            };
        }

        public async Task<(bool success, string message)> CancelBookingWithReason(int bookingId, string cancelReason)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.Payment)
                    .ThenInclude(p => p!.PaymentStatus)
                .Include(b => b.User)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy đặt phòng.");

            // Chỉ cho phép hủy phòng ở trạng thái "Chờ xác nhận" hoặc "Đã xác nhận"
            if (booking.BookingStatus.Name == "Hoàn thành")
                return (false, "Không thể hủy đặt phòng đã hoàn thành.");

            if (booking.BookingStatus.Name == "Đã hủy")
                return (false, "Đặt phòng này đã được hủy trước đó.");

            // Kiểm tra logic thanh toán - với flow mới chỉ thanh toán tại khách sạn
            if (booking.Payment != null && booking.Payment.PaymentStatus.Name == "Thành công")
            {
                return (false, "Không thể hủy đặt phòng đã thanh toán thành công. Vui lòng liên hệ quản lý để xử lý.");
            }

            // Cập nhật trạng thái thành "Đã hủy"
            var cancelledStatus = await _context.BookingStatuses.FirstOrDefaultAsync(s => s.Name == "Đã hủy");
            if (cancelledStatus == null)
                return (false, "Không tìm thấy trạng thái 'Đã hủy' trong hệ thống.");

            booking.BookingStatus = cancelledStatus;

            // Cập nhật trạng thái phòng về khả dụng
            if (booking.Room != null)
            {
                booking.Room.IsAvailable = true;
                _context.Rooms.Update(booking.Room);
            }

            _context.Update(booking);
            await _context.SaveChangesAsync();

            // Gửi thông báo real-time cho khách hàng về việc hủy đặt phòng
            if (booking.User != null)
            {
                var customerMessage = $"Đặt phòng #{bookingId} tại {booking.Room?.Name} đã bị hủy. Lý do: {cancelReason}";

                await _notificationService.SendBookingStatusUpdateToCustomerAsync(
                    booking.User.Id,
                    bookingId.ToString(),
                    "Đã hủy",
                    customerMessage
                );
            }

            return (true, "Đã hủy đặt phòng thành công và thông báo khách hàng.");
        }
    }
}
