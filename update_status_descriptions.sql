-- Cập nhật mô tả BookingStatus theo logic mới
-- Tách biệt rõ ràng giữa xác nhận đặt phòng và thanh toán

UPDATE BookingStatuses 
SET Description = 'Đơn đặt phòng đang chờ xác nhận từ khách sạn'
WHERE Name = N'Chờ xác nhận';

UPDATE BookingStatuses 
SET Description = N'Đơn đặt phòng đã được xác nhận, phòng sẵn sàng phục vụ khách hàng'
WHERE Name = N'Đã xác nhận';

UPDATE BookingStatuses 
SET Description = N'Khách hàng đã check-out thành công và hoàn tất thanh toán'
WHERE Name = N'Hoàn thành';

UPDATE BookingStatuses 
SET Description = N'Đơn đặt phòng đã bị hủy bởi khách hàng hoặc khách sạn'
WHERE Name = N'Đã hủy';

-- Cập nhật mô tả PaymentStatus để rõ ràng hơn

UPDATE PaymentStatuses 
SET Description = N'Chưa thanh toán hoặc đang xử lý thanh toán'
WHERE Name = N'Đang xử lý';

UPDATE PaymentStatuses 
SET Description = N'Đã thanh toán đầy đủ và thành công'
WHERE Name = N'Thành công';

UPDATE PaymentStatuses 
SET Description = N'Thanh toán không thành công'
WHERE Name = N'Thất bại';

UPDATE PaymentStatuses 
SET Description = N'Đã hoàn tiền cho khách hàng'
WHERE Name = N'Đã hoàn tiền';

-- Kiểm tra kết quả
SELECT 'BookingStatuses' as TableName, Name, Description FROM BookingStatuses
UNION ALL
SELECT 'PaymentStatuses' as TableName, Name, Description FROM PaymentStatuses
ORDER BY TableName, Name;
