using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Interfaces;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace HotelBookingSystem.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendBookingConfirmationToCustomerAsync(Booking booking)
        {
            try
            {
                string subject = $"Xác nhận đặt phòng #{booking.Id} - {booking.Room.Name}";
                string body = GenerateCustomerConfirmationEmail(booking);

                await SendEmailAsync(booking.User.Email ?? "", subject, body);
                _logger.LogInformation($"Booking confirmation email sent to customer: {booking.User.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking confirmation email to customer: {booking.User.Email}");
                throw;
            }
        }

        public async Task SendBookingNotificationToHotelAsync(Booking booking)
        {
            try
            {
                string hotelEmail = _configuration["HotelSettings:Email"] ?? "hotel@example.com";
                string subject = $"Đặt phòng mới #{booking.Id} - {booking.Room.Name}";
                string body = GenerateHotelNotificationEmail(booking);

                await SendEmailAsync(hotelEmail, subject, body);
                _logger.LogInformation($"Booking notification email sent to hotel: {hotelEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking notification email to hotel");
                throw;
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                using var client = new SmtpClient(smtpSettings["Host"] ?? "localhost", int.Parse(smtpSettings["Port"] ?? "587"));
                client.EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
                client.Credentials = new NetworkCredential(
                    smtpSettings["Username"] ?? "",
                    smtpSettings["Password"] ?? ""
                );

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? "noreply@hotel.com", smtpSettings["FromName"] ?? "Hotel Booking System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to: {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to: {to}");
                throw;
            }
        }

        private string GenerateCustomerConfirmationEmail(Booking booking)
        {
            var hotelSettings = _configuration.GetSection("HotelSettings");
            string hotelEmail = hotelSettings["Email"] ?? "contact@hotel.com";
            string hotelPhone = hotelSettings["Phone"] ?? "(84) 123-456-789";
            string hotelAddress = hotelSettings["Address"] ?? "123 Đường ABC, Quận 1, TP.HCM";

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .booking-details {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #007bff; }}
                        .detail-row {{ display: flex; justify-content: space-between; margin: 10px 0; }}
                        .label {{ font-weight: bold; }}
                        .footer {{ background-color: #333; color: white; padding: 15px; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏨 Xác nhận đặt phòng thành công!</h1>
                        </div>
                        
                        <div class='content'>
                            <h2>Xin chào {booking.User.FullName},</h2>
                            <p>Cảm ơn bạn đã đặt phòng tại khách sạn của chúng tôi. Đặt phòng của bạn đã được xác nhận thành công!</p>
                            
                            <div class='booking-details'>
                                <h3>📋 Thông tin đặt phòng</h3>
                                <div class='detail-row'>
                                    <span class='label'>Mã đặt phòng:</span>
                                    <span>#{booking.Id}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Tên phòng:</span>
                                    <span>{booking.Room.Name}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Loại phòng:</span>
                                    <span>{booking.Room.RoomType}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Ngày nhận phòng:</span>
                                    <span>{booking.CheckIn:dd/MM/yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Ngày trả phòng:</span>
                                    <span>{booking.CheckOut:dd/MM/yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Số khách:</span>
                                    <span>{booking.Guests} người</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Tổng tiền:</span>
                                    <span style='color: #007bff; font-weight: bold;'>{booking.TotalPrice:N0} VNĐ</span>
                                </div>
                            </div>
                            
                            <div class='booking-details'>
                                <h3>💳 Thông tin thanh toán</h3>
                                <p><strong>Phương thức:</strong> Thanh toán tại khách sạn bằng tiền mặt</p>
                                <p><strong>Trạng thái:</strong> Chờ thanh toán khi nhận phòng</p>
                                <p style='color: #28a745;'><strong>✅ Miễn phí đặt phòng - Không thu phí trước!</strong></p>
                            </div>
                            
                            <div class='booking-details'>
                                <h3>📞 Thông tin liên hệ</h3>
                                <p><strong>Hotline:</strong> {hotelPhone}</p>
                                <p><strong>Email:</strong> {hotelEmail}</p>
                                <p><strong>Địa chỉ:</strong> {hotelAddress}</p>
                            </div>
                            
                            <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 15px 0;'>
                                <h4>⚠️ Lưu ý quan trọng:</h4>
                                <ul>
                                    <li>Có thể hủy miễn phí trước 6 giờ so với giờ nhận phòng</li>
                                    <li>Vui lòng mang theo giấy tờ tùy thân khi nhận phòng</li>
                                    <li>Thanh toán bằng tiền mặt tại quầy lễ tân</li>
                                    <li>Giờ nhận phòng: 14:00 | Giờ trả phòng: 12:00</li>
                                </ul>
                            </div>
                        </div>
                        
                        <div class='footer'>
                            <p>Cảm ơn bạn đã chọn khách sạn của chúng tôi!</p>
                            <p>Chúc bạn có một kỳ nghỉ tuyệt vời! 🌟</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateHotelNotificationEmail(Booking booking)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .booking-details {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #28a745; }}
                        .detail-row {{ display: flex; justify-content: space-between; margin: 10px 0; }}
                        .label {{ font-weight: bold; }}
                        .urgent {{ background-color: #dc3545; color: white; padding: 10px; text-align: center; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏨 Đặt phòng mới!</h1>
                        </div>
                        
                        <div class='urgent'>
                            <strong>🔔 THÔNG BÁO MỚI: Có khách đặt phòng!</strong>
                        </div>
                        
                        <div class='content'>
                            <h2>Đặt phòng mới #{booking.Id}</h2>
                            <p>Có một đặt phòng mới vừa được tạo trên hệ thống. Vui lòng kiểm tra và chuẩn bị phòng.</p>
                            
                            <div class='booking-details'>
                                <h3>📋 Thông tin đặt phòng</h3>
                                <div class='detail-row'>
                                    <span class='label'>Mã đặt phòng:</span>
                                    <span>#{booking.Id}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Ngày đặt:</span>
                                    <span>{booking.CreatedDate:dd/MM/yyyy HH:mm}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Tên phòng:</span>
                                    <span>{booking.Room.Name}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Loại phòng:</span>
                                    <span>{booking.Room.RoomType}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Ngày nhận phòng:</span>
                                    <span>{booking.CheckIn:dd/MM/yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Ngày trả phòng:</span>
                                    <span>{booking.CheckOut:dd/MM/yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Số khách:</span>
                                    <span>{booking.Guests} người</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Tổng tiền:</span>
                                    <span style='color: #28a745; font-weight: bold;'>{booking.TotalPrice:N0} VNĐ</span>
                                </div>
                            </div>
                            
                            <div class='booking-details'>
                                <h3>👤 Thông tin khách hàng</h3>
                                <div class='detail-row'>
                                    <span class='label'>Họ tên:</span>
                                    <span>{booking.User.FullName}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Email:</span>
                                    <span>{booking.User.Email}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Số điện thoại:</span>
                                    <span>{booking.User.PhoneNumber}</span>
                                </div>
                            </div>
                            
                            <div class='booking-details'>
                                <h3>💳 Thông tin thanh toán</h3>
                                <p><strong>Phương thức:</strong> Thanh toán tại khách sạn</p>
                                <p><strong>Trạng thái:</strong> Chờ thanh toán khi nhận phòng</p>
                                <p style='color: #dc3545;'><strong>⚠️ Khách sẽ thanh toán bằng tiền mặt khi check-in</strong></p>
                            </div>
                            
                            <div style='background-color: #d4edda; padding: 15px; border-left: 4px solid #28a745; margin: 15px 0;'>
                                <h4>✅ Cần làm:</h4>
                                <ul>
                                    <li>Kiểm tra tình trạng phòng {booking.Room.Name}</li>
                                    <li>Chuẩn bị phòng cho ngày {booking.CheckIn:dd/MM/yyyy}</li>
                                    <li>Liên hệ khách hàng nếu cần thiết</li>
                                    <li>Cập nhật trạng thái đặt phòng trong hệ thống</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        #region Status Change Email Methods

        public async Task SendBookingStatusChangeToCustomerAsync(Booking booking, string oldStatus, string newStatus)
        {
            try
            {
                string subject = $"Thay đổi trạng thái đặt phòng #{booking.Id}";
                string body = GenerateBookingStatusChangeEmailToCustomer(booking, oldStatus, newStatus);

                await SendEmailAsync(booking.User?.Email ?? "", subject, body);
                _logger.LogInformation($"Booking status change email sent to customer: {booking.User?.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking status change email to customer: {booking.User?.Email}");
                throw;
            }
        }

        public async Task SendBookingStatusChangeToHotelAsync(Booking booking, string oldStatus, string newStatus)
        {
            try
            {
                var hotelEmail = _configuration["HotelSettings:Email"] ?? "annthe176161@fpt.edu.vn";
                string subject = $"Thay đổi trạng thái đặt phòng #{booking.Id}";
                string body = GenerateBookingStatusChangeEmailToHotel(booking, oldStatus, newStatus);

                await SendEmailAsync(hotelEmail, subject, body);
                _logger.LogInformation($"Booking status change email sent to hotel: {hotelEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking status change email to hotel");
                throw;
            }
        }

        public async Task SendPaymentStatusChangeToCustomerAsync(Booking booking, string oldPaymentStatus, string newPaymentStatus)
        {
            try
            {
                string subject = $"Thay đổi trạng thái thanh toán - Đặt phòng #{booking.Id}";
                string body = GeneratePaymentStatusChangeEmailToCustomer(booking, oldPaymentStatus, newPaymentStatus);

                await SendEmailAsync(booking.User?.Email ?? "", subject, body);
                _logger.LogInformation($"Payment status change email sent to customer: {booking.User?.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send payment status change email to customer: {booking.User?.Email}");
                throw;
            }
        }

        public async Task SendPaymentStatusChangeToHotelAsync(Booking booking, string oldPaymentStatus, string newPaymentStatus)
        {
            try
            {
                var hotelEmail = _configuration["HotelSettings:Email"] ?? "annthe176161@fpt.edu.vn";
                string subject = $"Thay đổi trạng thái thanh toán - Đặt phòng #{booking.Id}";
                string body = GeneratePaymentStatusChangeEmailToHotel(booking, oldPaymentStatus, newPaymentStatus);

                await SendEmailAsync(hotelEmail, subject, body);
                _logger.LogInformation($"Payment status change email sent to hotel: {hotelEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send payment status change email to hotel");
                throw;
            }
        }

        public async Task SendBookingCancellationToCustomerAsync(Booking booking, string reason)
        {
            try
            {
                string subject = $"Hủy đặt phòng #{booking.Id}";
                string body = GenerateBookingCancellationEmailToCustomer(booking, reason);

                await SendEmailAsync(booking.User?.Email ?? "", subject, body);
                _logger.LogInformation($"Booking cancellation email sent to customer: {booking.User?.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking cancellation email to customer: {booking.User?.Email}");
                throw;
            }
        }

        public async Task SendBookingCancellationToHotelAsync(Booking booking, string reason)
        {
            try
            {
                var hotelEmail = _configuration["HotelSettings:Email"] ?? "annthe176161@fpt.edu.vn";
                string subject = $"Hủy đặt phòng #{booking.Id}";
                string body = GenerateBookingCancellationEmailToHotel(booking, reason);

                await SendEmailAsync(hotelEmail, subject, body);
                _logger.LogInformation($"Booking cancellation email sent to hotel: {hotelEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking cancellation email to hotel");
                throw;
            }
        }

        public async Task SendCheckInReminderToCustomerAsync(Booking booking)
        {
            try
            {
                string subject = $"Nhắc nhở check-in - Đặt phòng #{booking.Id}";
                string body = GenerateCheckInReminderEmail(booking);

                await SendEmailAsync(booking.User?.Email ?? "", subject, body);
                _logger.LogInformation($"Check-in reminder email sent to customer: {booking.User?.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send check-in reminder email to customer: {booking.User?.Email}");
                throw;
            }
        }

        public async Task SendPaymentReminderToCustomerAsync(Booking booking)
        {
            try
            {
                string subject = $"Nhắc nhở thanh toán - Đặt phòng #{booking.Id}";
                string body = GeneratePaymentReminderEmail(booking);

                await SendEmailAsync(booking.User?.Email ?? "", subject, body);
                _logger.LogInformation($"Payment reminder email sent to customer: {booking.User?.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send payment reminder email to customer: {booking.User?.Email}");
                throw;
            }
        }

        #endregion

        #region Email Template Generators

        private string GenerateBookingStatusChangeEmailToCustomer(Booking booking, string oldStatus, string newStatus)
        {
            var statusColor = newStatus.ToLower() switch
            {
                "confirmed" or "xác nhận" => "#28a745",
                "cancelled" or "hủy" => "#dc3545",
                "pending" or "chờ xử lý" => "#ffc107",
                "completed" or "hoàn thành" => "#17a2b8",
                _ => "#6c757d"
            };

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                        .content {{ padding: 30px; background: white; }}
                        .status-box {{ background: {statusColor}; color: white; padding: 15px; border-radius: 8px; text-align: center; margin: 20px 0; }}
                        .info-box {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 15px 0; }}
                        .footer {{ background: #343a40; color: white; padding: 20px; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏨 Cập nhật trạng thái đặt phòng</h1>
                        </div>
                        <div class='content'>
                            <h2>Chào {booking.User?.FullName ?? "Quý khách"},</h2>
                            
                            <p>Trạng thái đặt phòng của bạn đã được cập nhật!</p>
                            
                            <div class='status-box'>
                                <h3>Trạng thái mới: {newStatus.ToUpper()}</h3>
                                <p>Thay đổi từ: {oldStatus} → {newStatus}</p>
                            </div>
                            
                            <div class='info-box'>
                                <h4>📋 Thông tin đặt phòng</h4>
                                <p><strong>Mã đặt phòng:</strong> #{booking.Id}</p>
                                <p><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</p>
                                <p><strong>Ngày nhận phòng:</strong> {booking.CheckIn:dd/MM/yyyy}</p>
                                <p><strong>Ngày trả phòng:</strong> {booking.CheckOut:dd/MM/yyyy}</p>
                                <p><strong>Tổng tiền:</strong> {booking.TotalPrice:C}</p>
                            </div>
                            
                            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>
                        </div>
                        <div class='footer'>
                            <p>🏨 Hotel Booking System | Email: annthe176161@fpt.edu.vn</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateBookingStatusChangeEmailToHotel(Booking booking, string oldStatus, string newStatus)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #fff; border: 1px solid #ddd; }}
                        .header {{ background: #e74c3c; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .status-change {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏨 Thông báo thay đổi trạng thái đặt phòng</h1>
                        </div>
                        <div class='content'>
                            <h2>Thông báo hệ thống</h2>
                            
                            <div class='status-change'>
                                <h3>Thay đổi trạng thái: {oldStatus} → {newStatus}</h3>
                            </div>
                            
                            <h4>📋 Chi tiết đặt phòng:</h4>
                            <ul>
                                <li><strong>Mã đặt phòng:</strong> #{booking.Id}</li>
                                <li><strong>Khách hàng:</strong> {booking.User?.FullName ?? "N/A"}</li>
                                <li><strong>Email:</strong> {booking.User?.Email ?? "N/A"}</li>
                                <li><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</li>
                                <li><strong>Ngày nhận phòng:</strong> {booking.CheckIn:dd/MM/yyyy}</li>
                                <li><strong>Ngày trả phòng:</strong> {booking.CheckOut:dd/MM/yyyy}</li>
                                <li><strong>Tổng tiền:</strong> {booking.TotalPrice:C}</li>
                                <li><strong>Ngày tạo:</strong> {booking.CreatedDate:dd/MM/yyyy HH:mm}</li>
                            </ul>
                            
                            <p><em>Vui lòng cập nhật hệ thống và thực hiện các bước cần thiết.</em></p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePaymentStatusChangeEmailToCustomer(Booking booking, string oldPaymentStatus, string newPaymentStatus)
        {
            var statusColor = newPaymentStatus.ToLower() switch
            {
                "paid" or "đã thanh toán" => "#28a745",
                "pending" or "chờ thanh toán" => "#ffc107",
                "failed" or "thất bại" => "#dc3545",
                "refunded" or "đã hoàn tiền" => "#17a2b8",
                _ => "#6c757d"
            };

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; }}
                        .content {{ padding: 30px; background: white; }}
                        .payment-status {{ background: {statusColor}; color: white; padding: 15px; border-radius: 8px; text-align: center; margin: 20px 0; }}
                        .info-box {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>💳 Cập nhật trạng thái thanh toán</h1>
                        </div>
                        <div class='content'>
                            <h2>Chào {booking.User?.FullName ?? "Quý khách"},</h2>
                            
                            <p>Trạng thái thanh toán cho đặt phòng của bạn đã được cập nhật!</p>
                            
                            <div class='payment-status'>
                                <h3>Trạng thái thanh toán: {newPaymentStatus.ToUpper()}</h3>
                                <p>Thay đổi từ: {oldPaymentStatus} → {newPaymentStatus}</p>
                            </div>
                            
                            <div class='info-box'>
                                <h4>📋 Thông tin đặt phòng</h4>
                                <p><strong>Mã đặt phòng:</strong> #{booking.Id}</p>
                                <p><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</p>
                                <p><strong>Tổng tiền:</strong> {booking.TotalPrice:C}</p>
                                <p><strong>Ngày nhận phòng:</strong> {booking.CheckIn:dd/MM/yyyy}</p>
                            </div>
                            
                            <p>Cảm ơn bạn đã tin tưởng sử dụng dịch vụ của chúng tôi!</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePaymentStatusChangeEmailToHotel(Booking booking, string oldPaymentStatus, string newPaymentStatus)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #fff; border: 1px solid #ddd; }}
                        .header {{ background: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .payment-change {{ background: #d1ecf1; border: 1px solid #bee5eb; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>💳 Thông báo thay đổi trạng thái thanh toán</h1>
                        </div>
                        <div class='content'>
                            <h2>Thông báo hệ thống</h2>
                            
                            <div class='payment-change'>
                                <h3>Thanh toán: {oldPaymentStatus} → {newPaymentStatus}</h3>
                                <p><strong>Số tiền:</strong> {booking.TotalPrice:C}</p>
                            </div>
                            
                            <h4>📋 Chi tiết đặt phòng:</h4>
                            <ul>
                                <li><strong>Mã đặt phòng:</strong> #{booking.Id}</li>
                                <li><strong>Khách hàng:</strong> {booking.User?.FullName ?? "N/A"} ({booking.User?.Email ?? "N/A"})</li>
                                <li><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</li>
                                <li><strong>Tổng tiền:</strong> {booking.TotalPrice:C}</li>
                                <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                            </ul>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateBookingCancellationEmailToCustomer(Booking booking, string reason)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); color: white; padding: 30px; text-align: center; }}
                        .content {{ padding: 30px; background: white; }}
                        .cancellation-box {{ background: #f8d7da; border: 1px solid #f5c6cb; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .info-box {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>❌ Thông báo hủy đặt phòng</h1>
                        </div>
                        <div class='content'>
                            <h2>Chào {booking.User?.FullName ?? "Quý khách"},</h2>
                            
                            <p>Chúng tôi rất tiếc phải thông báo rằng đặt phòng của bạn đã bị hủy.</p>
                            
                            <div class='cancellation-box'>
                                <h3>🚫 Đặt phòng #{booking.Id} đã bị hủy</h3>
                                <p><strong>Lý do hủy:</strong> {reason}</p>
                            </div>
                            
                            <div class='info-box'>
                                <h4>📋 Thông tin đặt phòng đã hủy</h4>
                                <p><strong>Mã đặt phòng:</strong> #{booking.Id}</p>
                                <p><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</p>
                                <p><strong>Ngày nhận phòng:</strong> {booking.CheckIn:dd/MM/yyyy}</p>
                                <p><strong>Ngày trả phòng:</strong> {booking.CheckOut:dd/MM/yyyy}</p>
                                <p><strong>Tổng tiền:</strong> {booking.TotalPrice:C}</p>
                            </div>
                            
                            <p>Nếu bạn đã thanh toán, chúng tôi sẽ hoàn tiền trong vòng 3-5 ngày làm việc.</p>
                            <p>Xin lỗi vì sự bất tiện này. Vui lòng liên hệ với chúng tôi nếu có bất kỳ câu hỏi nào.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateBookingCancellationEmailToHotel(Booking booking, string reason)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #fff; border: 1px solid #ddd; }}
                        .header {{ background: #dc3545; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .cancellation {{ background: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>❌ Thông báo hủy đặt phòng</h1>
                        </div>
                        <div class='content'>
                            <h2>Thông báo hệ thống</h2>
                            
                            <div class='cancellation'>
                                <h3>🚫 Đặt phòng #{booking.Id} đã bị hủy</h3>
                                <p><strong>Lý do:</strong> {reason}</p>
                            </div>
                            
                            <h4>📋 Chi tiết đặt phòng đã hủy:</h4>
                            <ul>
                                <li><strong>Mã đặt phòng:</strong> #{booking.Id}</li>
                                <li><strong>Khách hàng:</strong> {booking.User?.FullName ?? "N/A"} ({booking.User?.Email ?? "N/A"})</li>
                                <li><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</li>
                                <li><strong>Ngày nhận phòng:</strong> {booking.CheckIn:dd/MM/yyyy}</li>
                                <li><strong>Ngày trả phòng:</strong> {booking.CheckOut:dd/MM/yyyy}</li>
                                <li><strong>Tổng tiền:</strong> {booking.TotalPrice:C}</li>
                                <li><strong>Thời gian hủy:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                            </ul>
                            
                            <p><strong>Lưu ý:</strong> Vui lòng cập nhật tình trạng phòng và xử lý hoàn tiền (nếu có).</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateCheckInReminderEmail(Booking booking)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #17a2b8 0%, #138496 100%); color: white; padding: 30px; text-align: center; }}
                        .content {{ padding: 30px; background: white; }}
                        .reminder-box {{ background: #d1ecf1; border: 1px solid #bee5eb; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .info-box {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏨 Nhắc nhở check-in</h1>
                        </div>
                        <div class='content'>
                            <h2>Chào {booking.User?.FullName ?? "Quý khách"},</h2>
                            
                            <div class='reminder-box'>
                                <h3>📅 Nhắc nhở check-in</h3>
                                <p>Bạn có lịch check-in vào ngày <strong>{booking.CheckIn:dd/MM/yyyy}</strong></p>
                            </div>
                            
                            <div class='info-box'>
                                <h4>📋 Thông tin đặt phòng</h4>
                                <p><strong>Mã đặt phòng:</strong> #{booking.Id}</p>
                                <p><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</p>
                                <p><strong>Ngày nhận phòng:</strong> {booking.CheckIn:dd/MM/yyyy}</p>
                                <p><strong>Ngày trả phòng:</strong> {booking.CheckOut:dd/MM/yyyy}</p>
                            </div>
                            
                            <p>Vui lòng chuẩn bị đầy đủ giấy tờ tùy thân để làm thủ tục check-in thuận lợi.</p>
                            <p>Chúng tôi rất mong được phục vụ bạn!</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePaymentReminderEmail(Booking booking)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #ffc107 0%, #e0a800 100%); color: #333; padding: 30px; text-align: center; }}
                        .content {{ padding: 30px; background: white; }}
                        .payment-reminder {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .info-box {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>💳 Nhắc nhở thanh toán</h1>
                        </div>
                        <div class='content'>
                            <h2>Chào {booking.User?.FullName ?? "Quý khách"},</h2>
                            
                            <div class='payment-reminder'>
                                <h3>⚠️ Nhắc nhở thanh toán</h3>
                                <p>Bạn có khoản thanh toán chưa hoàn thành cho đặt phòng #{booking.Id}</p>
                                <p><strong>Số tiền:</strong> {booking.TotalPrice:C}</p>
                            </div>
                            
                            <div class='info-box'>
                                <h4>📋 Thông tin đặt phòng</h4>
                                <p><strong>Mã đặt phòng:</strong> #{booking.Id}</p>
                                <p><strong>Phòng:</strong> {booking.Room?.Name ?? "N/A"}</p>
                                <p><strong>Ngày nhận phòng:</strong> {booking.CheckIn:dd/MM/yyyy}</p>
                                <p><strong>Tổng tiền:</strong> {booking.TotalPrice:C}</p>
                            </div>
                            
                            <p>Vui lòng hoàn tất thanh toán để đảm bảo đặt phòng của bạn.</p>
                            <p>Cảm ơn bạn đã tin tưởng sử dụng dịch vụ của chúng tôi!</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        #endregion
    }
}
