-- Thêm user vào role Admin nếu chưa có
IF NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles ur
    INNER JOIN AspNetUsers u ON ur.UserId = u.Id
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE u.Email = 'admin@hoteltest.com' AND r.Name = 'Admin'
)
BEGIN
    DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'admin@hoteltest.com')
    DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
    
    IF @UserId IS NOT NULL AND @RoleId IS NOT NULL
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)
        PRINT 'Added admin@hoteltest.com to Admin role'
    END
    ELSE
    BEGIN
        PRINT 'User or Role not found'
    END
END
ELSE
BEGIN
    PRINT 'User admin@hoteltest.com is already an Admin'
END

-- Kiểm tra kết quả
SELECT 
    u.Email,
    u.UserName,
    r.Name as RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@hoteltest.com'
