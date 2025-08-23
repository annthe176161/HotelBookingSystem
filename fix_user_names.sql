-- Script để cập nhật FirstName và LastName từ FullName cho tất cả users
-- Chạy script này trong SQL Server Management Studio hoặc trong Package Manager Console

-- Cập nhật FirstName và LastName từ FullName
UPDATE AspNetUsers 
SET 
    FirstName = CASE 
        WHEN CHARINDEX(' ', FullName) > 0 
        THEN LEFT(FullName, CHARINDEX(' ', FullName) - 1)
        ELSE FullName 
    END,
    LastName = CASE 
        WHEN CHARINDEX(' ', FullName) > 0 
        THEN LTRIM(SUBSTRING(FullName, CHARINDEX(' ', FullName) + 1, LEN(FullName)))
        ELSE '' 
    END
WHERE 
    FullName IS NOT NULL 
    AND FullName != ''
    AND (FirstName IS NULL OR FirstName = '' OR LastName IS NULL OR LastName = '');

-- Kiểm tra kết quả
SELECT Id, FullName, FirstName, LastName 
FROM AspNetUsers 
WHERE FullName IS NOT NULL AND FullName != '';
