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
    }
}
