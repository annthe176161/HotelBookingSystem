-- Xóa tất cả dữ liệu để seed lại
-- CẢNH BÁO: Điều này sẽ xóa tất cả dữ liệu!

-- Xóa dữ liệu theo thứ tự dependency
DELETE FROM Reviews;
DELETE FROM Payments;
DELETE FROM Bookings;
DELETE FROM Rooms;
DELETE FROM BookingStatuses;
DELETE FROM PaymentStatuses;

-- Reset Identity columns
DBCC CHECKIDENT ('Reviews', RESEED, 0);
DBCC CHECKIDENT ('Payments', RESEED, 0);
DBCC CHECKIDENT ('Bookings', RESEED, 0);
DBCC CHECKIDENT ('Rooms', RESEED, 0);
DBCC CHECKIDENT ('BookingStatuses', RESEED, 0);
DBCC CHECKIDENT ('PaymentStatuses', RESEED, 0);

-- Sau đó restart ứng dụng để seed lại
