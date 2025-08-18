using HotelBookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<ApplicationDbContext>();
            var logger = services.GetRequiredService<ILogger<SeedData>>();

            try
            {
                // Đảm bảo database được tạo
                await context.Database.MigrateAsync();

                // Seed Roles
                await SeedRolesAsync(roleManager, logger);

                // Seed Admin User
                await SeedAdminUserAsync(userManager, logger);

                // Seed Status Data
                await SeedBookingStatusesAsync(context, logger);
                await SeedPaymentStatusesAsync(context, logger);

                // Cập nhật dữ liệu Rooms thành tiếng Việt
                await UpdateRoomsToVietnameseAsync(context, logger);

                // Seed Default Test User
                await SeedDefaultTestUserAsync(userManager, roleManager, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private static async Task SeedDefaultTestUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            const string userEmail = "test.user@example.com";
            logger.LogInformation("Checking if default test user '{UserEmail}' exists...", userEmail);

            var testUser = await userManager.FindByEmailAsync(userEmail);
            if (testUser == null)
            {
                logger.LogInformation("Creating default test user '{UserEmail}'...", userEmail);
                testUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    FullName = "Test User",
                    PhoneNumber = "0123456789"
                };

                var result = await userManager.CreateAsync(testUser, "Password@123");
                if (result.Succeeded)
                {
                    logger.LogInformation("Default test user created successfully.");
                    // Gán vai trò "Guest"
                    if (await roleManager.RoleExistsAsync("Guest"))
                    {
                        await userManager.AddToRoleAsync(testUser, "Guest");
                        logger.LogInformation("Assigned 'Guest' role to default test user.");
                    }
                }
                else
                {
                    logger.LogError("Failed to create default test user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Default test user '{UserEmail}' already exists.", userEmail);
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            logger.LogInformation("Checking if Admin role exists...");

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                logger.LogInformation("Creating Admin role...");
                var result = await roleManager.CreateAsync(new IdentityRole("Admin"));

                if (result.Succeeded)
                {
                    logger.LogInformation("Admin role created successfully.");
                }
                else
                {
                    logger.LogError("Failed to create Admin role: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin role already exists.");
            }

            // Tạo role Guest nếu cần
            if (!await roleManager.RoleExistsAsync("Guest"))
            {
                logger.LogInformation("Creating Guest role...");
                var result = await roleManager.CreateAsync(new IdentityRole("Guest"));

                if (result.Succeeded)
                {
                    logger.LogInformation("Guest role created successfully.");
                }
                else
                {
                    logger.LogError("Failed to create Guest role: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            logger.LogInformation("Checking if admin user exists...");

            var adminUser = await userManager.FindByEmailAsync("admin@hotel.com");

            if (adminUser == null)
            {
                logger.LogInformation("Creating admin user...");

                adminUser = new ApplicationUser
                {
                    UserName = "admin@hotel.com",
                    Email = "admin@hotel.com",
                    EmailConfirmed = true,
                    FullName = "Administrator"
                };

                var createResult = await userManager.CreateAsync(adminUser, "Admin123!");

                if (createResult.Succeeded)
                {
                    logger.LogInformation("Admin user created successfully.");

                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Admin role assigned to user successfully.");
                    }
                    else
                    {
                        logger.LogError("Failed to assign Admin role: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists.");
            }
        }

        private static async Task SeedRoomsAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.Rooms.Any())
            {
                logger.LogInformation("Seeding sample rooms...");

                var rooms = new List<Room>
        {
            new Room
            {
                Name = "Phòng Deluxe Hướng Biển",
                Description = "Phòng sang trọng với tầm nhìn tuyệt đẹp ra biển và các tiện nghi hiện đại.",
                PricePerNight = 2000000m, // 2,000,000 VND
                ImageUrl = "/images/rooms/deluxe-ocean.jpg",
                Capacity = 2,
                RoomType = "Deluxe",
                IsAvailable = true,
                AverageRating = 4.5
            },
            new Room
            {
                Name = "Phòng Standard Giường Đôi",
                Description = "Phòng thoải mái với giường đôi, phù hợp cho khách công tác.",
                PricePerNight = 1200000m, // 1,200,000 VND
                ImageUrl = "/images/rooms/standard-twin.jpg",
                Capacity = 2,
                RoomType = "Standard",
                IsAvailable = true,
                AverageRating = 4.0
            },
            new Room
            {
                Name = "Phòng Suite Gia Đình",
                Description = "Phòng rộng rãi lý tưởng cho gia đình có trẻ em.",
                PricePerNight = 3500000m, // 3,500,000 VND
                ImageUrl = "/images/rooms/family-suite.jpg",
                Capacity = 4,
                RoomType = "Suite",
                IsAvailable = true,
                AverageRating = 4.8
            },
            new Room
            {
                Name = "Phòng Executive Business",
                Description = "Phòng chuyên nghiệp với bàn làm việc và internet tốc độ cao.",
                PricePerNight = 1800000m, // 1,800,000 VND
                ImageUrl = "/images/rooms/executive-business.jpg",
                Capacity = 1,
                RoomType = "Executive",
                IsAvailable = true,
                AverageRating = 4.3
            },
            new Room
            {
                Name = "Phòng Tổng Thống",
                Description = "Phòng cao cấp nhất với ban công riêng và dịch vụ premium.",
                PricePerNight = 8000000m, // 8,000,000 VND
                ImageUrl = "/images/rooms/presidential-suite.jpg",
                Capacity = 2,
                RoomType = "Presidential",
                IsAvailable = true,
                AverageRating = 5.0
            }
        };

                context.Rooms.AddRange(rooms);
                await context.SaveChangesAsync();

                logger.LogInformation("Sample rooms seeded successfully.");
            }
            else
            {
                logger.LogInformation("Rooms already exist in database.");
            }
        }

        private static async Task SeedBookingStatusesAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.BookingStatuses.Any())
            {
                logger.LogInformation("Seeding booking statuses...");

                var statuses = new List<BookingStatus>
        {
            new BookingStatus { Name = "Chờ xác nhận", Description = "Đơn đặt phòng đang chờ xác nhận", IsActive = true },
            new BookingStatus { Name = "Đã xác nhận", Description = "Đơn đặt phòng đã được xác nhận và thanh toán", IsActive = true },
            new BookingStatus { Name = "Hoàn thành", Description = "Khách đã check-out", IsActive = true },
            new BookingStatus { Name = "Đã hủy", Description = "Đơn đặt phòng đã bị hủy", IsActive = true }
        };

                context.BookingStatuses.AddRange(statuses);
                await context.SaveChangesAsync();

                logger.LogInformation("Booking statuses seeded successfully.");
            }
        }

        private static async Task SeedPaymentStatusesAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.PaymentStatuses.Any())
            {
                logger.LogInformation("Seeding payment statuses...");

                var statuses = new List<PaymentStatus>
        {
            new PaymentStatus { Name = "Đang xử lý", Description = "Thanh toán đang được xử lý", IsActive = true },
            new PaymentStatus { Name = "Thành công", Description = "Thanh toán thành công", IsActive = true },
            new PaymentStatus { Name = "Thất bại", Description = "Thanh toán thất bại", IsActive = true },
            new PaymentStatus { Name = "Đã hoàn tiền", Description = "Đã hoàn tiền", IsActive = true }
        };

                context.PaymentStatuses.AddRange(statuses);
                await context.SaveChangesAsync();

                logger.LogInformation("Payment statuses seeded successfully.");
            }
        }

        private static async Task UpdateRoomsToVietnameseAsync(ApplicationDbContext context, ILogger logger)
        {
            logger.LogInformation("Updating rooms to Vietnamese data...");

            // Xóa tất cả rooms hiện tại
            var existingRooms = context.Rooms.ToList();
            if (existingRooms.Any())
            {
                context.Rooms.RemoveRange(existingRooms);
                await context.SaveChangesAsync();
                logger.LogInformation("Removed {Count} existing rooms.", existingRooms.Count);
            }

            // Thêm rooms tiếng Việt
            var vietnameseRooms = new List<Room>
    {
        new Room
        {
            Name = "Phòng Deluxe Hướng Biển",
            Description = "Phòng sang trọng với tầm nhìn tuyệt đẹp ra biển và các tiện nghi hiện đại.",
            PricePerNight = 2000000m,
            ImageUrl = "https://movenpickresortcamranh.com/wp-content/uploads/2022/06/Movenpick-Resort-Cam-Ranh12.jpg",
            Capacity = 2,
            RoomType = "Deluxe",
            IsAvailable = true,
            AverageRating = 4.5
        },
        new Room
        {
            Name = "Phòng Standard Giường Đôi",
            Description = "Phòng thoải mái với giường đôi, phù hợp cho khách công tác.",
            PricePerNight = 1200000m,
            ImageUrl = "https://rosevalleydalat.com/wp-content/uploads/2019/05/Standard-double-1.jpg",
            Capacity = 2,
            RoomType = "Standard",
            IsAvailable = true,
            AverageRating = 4.0
        },
        new Room
        {
            Name = "Phòng Suite Gia Đình",
            Description = "Phòng rộng rãi lý tưởng cho gia đình có trẻ em.",
            PricePerNight = 3500000m,
            ImageUrl = "https://ezcloud.vn/wp-content/uploads/2023/10/family-suite-la-gi.webp",
            Capacity = 4,
            RoomType = "Suite",
            IsAvailable = true,
            AverageRating = 4.8
        },
        new Room
        {
            Name = "Phòng Executive Business",
            Description = "Phòng chuyên nghiệp với bàn làm việc và internet tốc độ cao.",
            PricePerNight = 1800000m,
            ImageUrl = "https://ezcloud.vn/wp-content/uploads/2023/10/phong-executive-la-gi.webp",
            Capacity = 1,
            RoomType = "Executive",
            IsAvailable = true,
            AverageRating = 4.3
        },
        new Room
        {
            Name = "Phòng Tổng Thống",
            Description = "Phòng cao cấp nhất với ban công riêng và dịch vụ premium.",
            PricePerNight = 8000000m,
            ImageUrl = "https://images2.thanhnien.vn/528068263637045248/2023/9/11/biden-16-169441443748282765858.jpg",
            Capacity = 2,
            RoomType = "Presidential",
            IsAvailable = true,
            AverageRating = 5.0
        }
    };

            context.Rooms.AddRange(vietnameseRooms);
            await context.SaveChangesAsync();

            logger.LogInformation("Added {Count} Vietnamese rooms successfully.", vietnameseRooms.Count);
        }
    }
}
