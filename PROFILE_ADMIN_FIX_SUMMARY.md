# Sửa Lỗi Profile và Admin Login - Summary

## Các vấn đề đã sửa:

### 1. **Profile Action bị Comment và Không Hoạt động**

**Vấn đề gốc:**

- Action `Profile()` trong `AccountController` bị comment
- Không thể truy cập trang Profile sau khi đăng nhập

**Giải pháp:**

- Uncomment và viết lại hoàn toàn action `Profile()`
- Thêm `[Authorize]` attribute để bảo vệ action
- Sử dụng `_userManager.GetUserAsync(User)` để lấy user hiện tại
- Thêm `using Microsoft.EntityFrameworkCore` để sử dụng `CountAsync()`

**Code mới:**

```csharp
[Authorize]
[HttpGet]
public async Task<IActionResult> Profile()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return RedirectToAction("Login");
    }

    // Tách FirstName / LastName từ FullName nếu có
    var firstName = user.FirstName ?? "";
    var lastName = user.LastName ?? "";

    if (string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(user.FullName))
    {
        var nameParts = user.FullName.Split(' ', 2);
        firstName = nameParts.Length > 0 ? nameParts[0] : "";
        lastName = nameParts.Length > 1 ? nameParts[1] : "";
    }

    var vm = new ProfileViewModel
    {
        FirstName = firstName,
        LastName = lastName,
        Email = user.Email ?? "",
        PhoneNumber = user.PhoneNumber ?? "",
        Birthdate = user.DateOfBirth ?? DateTime.Now.AddYears(-25),
        Gender = user.Gender == GenderType.Name ? "male" :
                user.Gender == GenderType.Nu ? "female" : "other",
        Address = user.Address ?? "",
        City = user.City ?? "",
        State = user.State ?? "",
        ZipCode = user.ZipCode ?? "",
        AvatarUrl = user.ProfilePictureUrl ?? "/images/default-avatar.png",
        CreatedAt = user.CreatedAt,

        // Đếm số booking và review
        BookingsCount = await _db.Bookings.CountAsync(b => b.UserId == user.Id),
        ReviewsCount = await _db.Reviews.CountAsync(r => r.UserId == user.Id),

        // Mock data cho loyalty system
        LoyaltyPoints = 250,
        LoyaltyTier = "Silver",
        NextTier = "Gold",
        PointsToNextTier = 150,
        NextTierProgress = 62,
        TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user)
    };

    return View(vm);
}
```

### 2. **Admin không được chuyển hướng đến Dashboard sau khi đăng nhập**

**Vấn đề gốc:**

- Logic đăng nhập không kiểm tra role Admin
- Tất cả user đều được chuyển về Home sau đăng nhập

**Giải pháp:**

- Cập nhật logic trong action `Login()` để kiểm tra role
- Thêm action `Index()` trong `AdminController` để handle routing mặc định

**Code cập nhật trong AccountController:**

```csharp
if (result.Succeeded)
{
    // Kiểm tra role để chuyển hướng phù hợp
    var roles = await _userManager.GetRolesAsync(user);

    // Nếu có returnUrl (đi từ trang đặt phòng) thì quay lại đó
    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        return LocalRedirect(returnUrl);

    // Nếu là admin thì chuyển đến admin dashboard
    if (roles.Contains("Admin"))
    {
        return RedirectToAction("Index", "Admin");
    }

    // Người dùng thường thì về Home
    return RedirectToAction("Index", "Home");
}
```

**Code thêm trong AdminController:**

```csharp
[HttpGet("")]
[HttpGet("Index")]
public IActionResult Index()
{
    // Chuyển hướng đến Dashboard
    return RedirectToAction("Dashboard");
}
```

## Tài khoản test:

### **Tài khoản Admin:**

- **Email**: `admin@luxuryhotel.com`
- **Password**: `Admin123!@#`
- **Sau đăng nhập**: Tự động chuyển đến `/Admin/Dashboard`

### **Tài khoản Customer:**

- **Email**: `test.customer@example.com`
- **Password**: `Test123!@#`
- **Sau đăng nhập**: Chuyển đến trang chủ hoặc returnUrl
- **Profile**: Có thể truy cập `/Account/Profile`

## Test Cases thành công:

### **Test 1: Admin Login**

1. ✅ Vào http://localhost:5265/Account/Login
2. ✅ Đăng nhập với `admin@luxuryhotel.com` / `Admin123!@#`
3. ✅ Chuyển hướng tự động đến `/Admin/Dashboard`

### **Test 2: Customer Login & Profile**

1. ✅ Vào http://localhost:5265/Account/Login
2. ✅ Đăng nhập với `test.customer@example.com` / `Test123!@#`
3. ✅ Chuyển hướng đến trang chủ
4. ✅ Click vào dropdown user → "Hồ sơ"
5. ✅ Truy cập thành công trang Profile với thông tin user

### **Test 3: Return URL khi đặt phòng**

1. ✅ Chưa đăng nhập, vào trang đặt phòng
2. ✅ Chuyển hướng đến login với returnUrl
3. ✅ Đăng nhập thành công
4. ✅ Quay lại trang đặt phòng ban đầu

## Các file đã thay đổi:

1. **Controllers/AccountController.cs**

   - ✅ Thêm logic kiểm tra role trong Login action
   - ✅ Tạo lại Profile action hoàn chỉnh
   - ✅ Thêm using Microsoft.EntityFrameworkCore

2. **Controllers/AdminController.cs**
   - ✅ Thêm action Index() để handle route mặc định

## Lưu ý kỹ thuật:

- **Gender Mapping**: Xử lý enum `GenderType` sang string cho ViewModel
- **Entity Framework**: Sử dụng `CountAsync()` thay vì `Count()` cho async operation
- **Null Safety**: Xử lý tất cả nullable properties với `?? ""` hoặc giá trị mặc định
- **Authorization**: Tất cả protected actions đều có `[Authorize]` attribute

## URL ứng dụng:

- **Local**: http://localhost:5265
- **Login**: http://localhost:5265/Account/Login
- **Profile**: http://localhost:5265/Account/Profile (cần đăng nhập)
- **Admin Dashboard**: http://localhost:5265/Admin/Dashboard (cần role Admin)
