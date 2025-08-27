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

                // Update Status Descriptions với logic mới
                await UpdateStatusDescriptionsAsync(context, logger);

                // Cập nhật dữ liệu Rooms thành tiếng Việt
                await UpdateRoomsToVietnameseAsync(context, logger);

                // Seed Default Test User
                await SeedDefaultTestUserAsync(userManager, roleManager, logger);

                // Seed Test Bookings
                await SeedTestBookingsAsync(context, userManager, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private static async Task SeedDefaultTestUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            // Xóa user cũ nếu tồn tại
            var oldUser = await userManager.FindByEmailAsync("test.user@example.com");
            if (oldUser != null)
            {
                logger.LogInformation("Removing old test user 'test.user@example.com'...");
                await userManager.DeleteAsync(oldUser);
            }

            const string userEmail = "test.customer@example.com";
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
                    FullName = "Test Customer",
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
            const string adminEmail = "admin@hoteltest.com";
            const string adminPassword = "Test@123";

            logger.LogInformation("Seeding admin user '{AdminEmail}'...", adminEmail);

            // Xóa admin user cũ nếu tồn tại để reset mật khẩu
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                logger.LogInformation("Admin user '{AdminEmail}' already exists. Removing to reset password.", adminEmail);
                var deletionResult = await userManager.DeleteAsync(existingAdmin);
                if (!deletionResult.Succeeded)
                {
                    logger.LogError("Failed to remove existing admin user: {Errors}", string.Join(", ", deletionResult.Errors.Select(e => e.Description)));
                    return; // Dừng lại nếu không xóa được
                }
                logger.LogInformation("Existing admin user removed successfully.");
            }

            // Tạo admin user mới
            logger.LogInformation("Creating new admin user '{AdminEmail}'...", adminEmail);
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Hotel Administrator",
                PhoneNumber = "0987654321",
                IsActivated = true
            };

            var creationResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (creationResult.Succeeded)
            {
                logger.LogInformation("Admin user created successfully.");

                // Gán vai trò "Admin"
                logger.LogInformation("Assigning 'Admin' role to the new admin user...");
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation("'Admin' role assigned successfully.");
                }
                else
                {
                    logger.LogError("Failed to assign 'Admin' role: {Errors}", string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", creationResult.Errors.Select(e => e.Description)));
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
            new BookingStatus { Name = "Chờ xác nhận", Description = "Đơn đặt phòng đang chờ xác nhận từ khách sạn", IsActive = true },
            new BookingStatus { Name = "Đã xác nhận", Description = "Đơn đặt phòng đã được xác nhận, phòng sẵn sàng phục vụ khách hàng", IsActive = true },
            new BookingStatus { Name = "Hoàn thành", Description = "Khách hàng đã check-out thành công và hoàn tất thanh toán", IsActive = true },
            new BookingStatus { Name = "Đã hủy", Description = "Đơn đặt phòng đã bị hủy bởi khách hàng hoặc khách sạn", IsActive = true }
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
            new PaymentStatus { Name = "Đang xử lý", Description = "Chưa thanh toán hoặc đang xử lý thanh toán", IsActive = true },
            new PaymentStatus { Name = "Thành công", Description = "Đã thanh toán đầy đủ và thành công", IsActive = true }
        };

                context.PaymentStatuses.AddRange(statuses);
                await context.SaveChangesAsync();

                logger.LogInformation("Payment statuses seeded successfully.");
            }
        }

        private static async Task UpdateStatusDescriptionsAsync(ApplicationDbContext context, ILogger logger)
        {
            logger.LogInformation("Updating status descriptions with new business logic...");

            // Cập nhật BookingStatus descriptions
            var bookingStatuses = await context.BookingStatuses.ToListAsync();

            var pendingStatus = bookingStatuses.FirstOrDefault(s => s.Name == "Chờ xác nhận");
            if (pendingStatus != null)
            {
                pendingStatus.Description = "Đơn đặt phòng đang chờ xác nhận từ khách sạn";
                context.Update(pendingStatus);
            }

            var confirmedStatus = bookingStatuses.FirstOrDefault(s => s.Name == "Đã xác nhận");
            if (confirmedStatus != null)
            {
                confirmedStatus.Description = "Đơn đặt phòng đã được xác nhận, phòng sẵn sàng phục vụ khách hàng";
                context.Update(confirmedStatus);
            }

            var completedStatus = bookingStatuses.FirstOrDefault(s => s.Name == "Hoàn thành");
            if (completedStatus != null)
            {
                completedStatus.Description = "Khách hàng đã check-out thành công và hoàn tất thanh toán";
                context.Update(completedStatus);
            }

            var cancelledStatus = bookingStatuses.FirstOrDefault(s => s.Name == "Đã hủy");
            if (cancelledStatus != null)
            {
                cancelledStatus.Description = "Đơn đặt phòng đã bị hủy bởi khách hàng hoặc khách sạn";
                context.Update(cancelledStatus);
            }

            // Cập nhật PaymentStatus descriptions
            var paymentStatuses = await context.PaymentStatuses.ToListAsync();

            var processingStatus = paymentStatuses.FirstOrDefault(s => s.Name == "Đang xử lý");
            if (processingStatus != null)
            {
                processingStatus.Description = "Chưa thanh toán hoặc đang xử lý thanh toán";
                context.Update(processingStatus);
            }

            var successStatus = paymentStatuses.FirstOrDefault(s => s.Name == "Thành công");
            if (successStatus != null)
            {
                successStatus.Description = "Đã thanh toán đầy đủ và thành công";
                context.Update(successStatus);
            }

            // Xóa các trạng thái không cần thiết nếu tồn tại
            var obsoleteStatuses = paymentStatuses.Where(s => s.Name == "Thất bại" || s.Name == "Đã hoàn tiền").ToList();
            if (obsoleteStatuses.Any())
            {
                context.PaymentStatuses.RemoveRange(obsoleteStatuses);
                logger.LogInformation($"Removed {obsoleteStatuses.Count} obsolete payment statuses: {string.Join(", ", obsoleteStatuses.Select(s => s.Name))}");
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Status descriptions updated successfully with new business logic.");
        }

        private static async Task SeedTestBookingsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            // Skip if already seeded
            if (context.Bookings.Any())
            {
                logger.LogInformation("Test bookings already exist.");
                return;
            }

            // Create test users if they don't exist
            if (context.Bookings.Any())
            {
                logger.LogInformation("Test bookings already exist.");
                return;
            }

            // Ensure test users exist and collect their references
            var testUserEmails = Enumerable.Range(1, 5).Select(i => $"test.user{i}@example.com").ToList();
            var testUsers = new List<ApplicationUser>();
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Guest");

            foreach (var email in testUserEmails)
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = $"Test User {email.Split('@')[0].Split('.').Last()}",
                        PhoneNumber = $"0123456{email.GetHashCode() % 1000:D3}",
                        FirstName = "Test",
                        LastName = email.Split('@')[0].Split('.').Last(),
                        IsActivated = true
                    };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();

                    if (userRole != null && !context.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == userRole.Id))
                    {
                        context.UserRoles.Add(new IdentityUserRole<string>
                        {
                            UserId = user.Id,
                            RoleId = userRole.Id
                        });
                        await context.SaveChangesAsync();
                    }
                }
                testUsers.Add(user);
            }

            await context.SaveChangesAsync();

            // Get all rooms
            var rooms = await context.Rooms.ToListAsync();
            if (!rooms.Any())
            {
                logger.LogWarning("No rooms found to create bookings.");
                return;
            }

            // Get booking statuses
            var pendingStatus = await context.BookingStatuses.FirstOrDefaultAsync(bs => bs.Name == "Chờ xác nhận");
            var confirmedStatus = await context.BookingStatuses.FirstOrDefaultAsync(bs => bs.Name == "Đã xác nhận");
            var completedStatus = await context.BookingStatuses.FirstOrDefaultAsync(bs => bs.Name == "Hoàn thành");
            var cancelledStatus = await context.BookingStatuses.FirstOrDefaultAsync(bs => bs.Name == "Đã hủy");

            // Get payment statuses
            var pendingPayment = await context.PaymentStatuses.FirstOrDefaultAsync(ps => ps.Name == "Đang xử lý");
            var successPayment = await context.PaymentStatuses.FirstOrDefaultAsync(ps => ps.Name == "Thành công");

            var random = new Random();
            var bookings = new List<Booking>();

            // Create 20 bookings with various scenarios
            for (int i = 0; i < 20; i++)
            {
                var room = rooms[random.Next(rooms.Count)];
                var user = testUsers[random.Next(testUsers.Count)];
                var daysOffset = random.Next(-30, 30); // Bookings from past 30 days to next 30 days
                var lengthOfStay = random.Next(1, 5);

                var checkIn = DateTime.Today.AddDays(daysOffset);
                var checkOut = checkIn.AddDays(lengthOfStay);
                var createdDate = checkIn.AddDays(-random.Next(1, 10));

                var booking = new Booking
                {
                    UserId = user.Id,
                    RoomId = room.Id,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Guests = random.Next(1, room.Capacity + 1),
                    TotalPrice = room.PricePerNight * lengthOfStay,
                    CreatedDate = createdDate
                };

                // Assign status based on dates
                if (checkIn < DateTime.Today && checkOut < DateTime.Today)
                {
                    // Past booking
                    booking.BookingStatusId = completedStatus?.Id ?? 1;
                    booking.CompletedDate = checkOut;
                }
                else if (checkIn > DateTime.Today)
                {
                    // Future booking
                    booking.BookingStatusId = random.Next(0, 10) < 8
                        ? (confirmedStatus?.Id ?? 2) // 80% confirmed
                        : (pendingStatus?.Id ?? 1);  // 20% pending
                }
                else
                {
                    // Current booking
                    booking.BookingStatusId = confirmedStatus?.Id ?? 2;
                }

                bookings.Add(booking);
                context.Bookings.Add(booking);
            }

            await context.SaveChangesAsync();

            // Create payments for bookings
            foreach (var booking in bookings)
            {
                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    PaymentMethod = "Cash",
                    TransactionId = $"TXN_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                    PaymentDate = booking.CreatedDate.AddMinutes(30),
                    PaymentStatusId = booking.BookingStatusId == completedStatus?.Id
                        ? (successPayment?.Id ?? 2)
                        : (pendingPayment?.Id ?? 1)
                };

                context.Payments.Add(payment);

                // Add review for completed bookings (70% chance)
                if (booking.BookingStatusId == completedStatus?.Id && random.Next(0, 10) < 7)
                {
                    var review = new Review
                    {
                        BookingId = booking.Id,
                        UserId = booking.UserId,
                        RoomId = booking.RoomId,
                        Rating = random.Next(3, 6), // 3-5 stars
                        Comment = $"Great stay at {booking.Room.Name}! Would recommend.",
                        CreatedDate = booking.CheckOut.AddDays(random.Next(1, 5))
                    };

                    context.Reviews.Add(review);
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Created test bookings, payments, and reviews successfully.");
        }

        private static async Task UpdateRoomsToVietnameseAsync(ApplicationDbContext context, ILogger logger)
        {
            logger.LogInformation("Updating rooms to Vietnamese data...");

            // Remove existing rooms
            var existingRooms = context.Rooms.ToList();
            if (existingRooms.Any())
            {
                context.Rooms.RemoveRange(existingRooms);
                await context.SaveChangesAsync();
            }

            // Add 20 Vietnamese rooms
            var rooms = new List<Room>
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
        },
                // Original 5 rooms plus 15 more...
                // (Previous room definitions)
                new Room
                {
                    Name = "Phòng Deluxe Hướng Biển",
                    Description = "Phòng sang trọng với tầm nhìn tuyệt đẹp ra biển.",
                    PricePerNight = 2000000m,
                    ImageUrl = "https://media.istockphoto.com/id/627892060/photo/hotel-room-suite-with-view.jpg?s=612x612&w=0&k=20&c=YBwxnGH3MkOLLpBKCvWAD8F__T-ypznRUJ_N13Zb1cU=",
                    Capacity = 2,
                    RoomType = "Deluxe",
                    IsAvailable = true,
                    AverageRating = 4.5
                },
                // Add more rooms here...
                new Room
                {
                    Name = "Phòng Suite Gia Đình Cao Cấp",
                    Description = "Suite rộng rãi dành cho gia đình với view panorama.",
                    PricePerNight = 3500000m,
                    ImageUrl = "https://media.istockphoto.com/id/627892060/photo/hotel-room-suite-with-view.jpg?s=612x612&w=0&k=20&c=YBwxnGH3MkOLLpBKCvWAD8F__T-ypznRUJ_N13Zb1cU=",
                    Capacity = 4,
                    RoomType = "Suite",
                    IsAvailable = true,
                    AverageRating = 4.8
                },
                // Continue adding more rooms...
            };

            // Add 15 more rooms with variations
            var roomTypes = new[] { "Standard", "Deluxe", "Suite", "Executive", "Family" };
            var random = new Random();

            for (int i = 6; i <= 20; i++)
            {
                var roomType = roomTypes[random.Next(roomTypes.Length)];
                var room = new Room
                {
                    Name = $"Phòng {roomType} {i}",
                    Description = $"Phòng {roomType.ToLower()} tiện nghi với thiết kế hiện đại.",
                    PricePerNight = (decimal)(1000000 + random.Next(500, 5000) * 1000),
                    ImageUrl = "https://media.istockphoto.com/id/627892060/photo/hotel-room-suite-with-view.jpg?s=612x612&w=0&k=20&c=YBwxnGH3MkOLLpBKCvWAD8F__T-ypznRUJ_N13Zb1cU=",
                    Capacity = random.Next(1, 5),
                    RoomType = roomType,
                    IsAvailable = true,
                    AverageRating = Math.Round(3.5 + random.NextDouble() * 1.5, 1)
                };
                rooms.Add(room);
            }

            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();

            logger.LogInformation($"Added {rooms.Count} Vietnamese rooms successfully.");
        }
    }
}
