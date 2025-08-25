using CloudinaryDotNet;
using HotelBookingSystem.Data;
using HotelBookingSystem.Infrastructure.Hubs;
using HotelBookingSystem.Infrastructure.Options;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services.Implementations;
using HotelBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using HotelBookingSystem.Worker;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HotelBookingSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                //options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(opts =>
                {
                    opts.SignIn.RequireConfirmedAccount = false;
                    opts.User.RequireUniqueEmail = true;
                    opts.Password.RequiredLength = 6;
                    opts.SignIn.RequireConfirmedEmail = false;
                    opts.Lockout.MaxFailedAccessAttempts = 5;
                    opts.User.AllowedUserNameCharacters = null;
                    opts.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            //worker
            builder.Services.AddHostedService<BookingExpirationWorker>();


            // Add SignalR
            builder.Services.AddSignalR();
            builder.Services.Configure<CloudinarySettings>(
                builder.Configuration.GetSection("Cloudinary"));
            builder.Services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                var account = new Account(opts.CloudName, opts.ApiKey, opts.ApiSecret);
                var cld = new Cloudinary(account);
                cld.Api.Secure = true;
                return cld;
            });
            builder.Services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();

            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddScoped<IAdminRoomService, AdminRoomService>();
            builder.Services.AddScoped<IAdminUserService, AdminUserService>();
            builder.Services.AddScoped<IAdminBookingService, AdminBookingService>();
            builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IBookingStatusService, BookingStatusService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            //Add google authentication
            builder.Services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["GoogleAuth:ClientId"] ?? "";
                options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"] ?? "";
                var cb = builder.Configuration["GoogleAuth:CallbackPath"];
                if (!string.IsNullOrWhiteSpace(cb)) options.CallbackPath = cb;
            });;

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Map SignalR Hub
            app.MapHub<NotificationHub>("/notificationHub");

            // Seed Database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await SeedData.InitializeAsync(services);

                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database: {ErrorMessage}", ex.Message);

                    Console.WriteLine($"Seed error: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            app.Run();
        }
    }
}
