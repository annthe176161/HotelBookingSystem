using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services.Implementations
{
    public interface IAdminUserService
    {
        Task<UsersViewModel> GetUsers(UserQueryOptions options);
        Task<IdentityResult> Add(CreateUserViewModel model, CancellationToken ct);
        Task<UserDetailsViewModel?> GetById(string id, CancellationToken ct = default);
        Task<IdentityResult> Update(EditUserViewModel model, CancellationToken ct = default);
    }

    public class AdminUserService : IAdminUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<UsersViewModel> GetUsers(UserQueryOptions options)
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");
            if (adminRole == null)
                throw new Exception("Admin role not found");

            var query = _context.Users
                .Include(u => u.Bookings)
                .Where(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRole.Id))
                .AsQueryable();

            var activeUsers = await query.CountAsync(u => u.IsActivated);
            var newUsers = await query.CountAsync(u => u.CreatedAt >= DateTime.Now.AddDays(-30));
            var bookingUsers = await query.CountAsync(u => u.Bookings.Any());
            var inactiveUsers = await query.CountAsync(u => !u.IsActivated);

            // Filters
            if (!string.IsNullOrEmpty(options.Name))
            {
                query = query.Where(u => u.FullName.Contains(options.Name));
            }
            if (!string.IsNullOrEmpty(options.Email))
            {
                query = query.Where(u => (u.Email ?? "").Contains(options.Email));
            }
            if (!string.IsNullOrEmpty(options.Phone))
            {
                query = query.Where(u => (u.PhoneNumber ?? "").Contains(options.Phone));
            }
            if (!string.IsNullOrEmpty(options.Status))
            {
                if (options.Status == "active")
                    query = query.Where(u => u.IsActivated);
                else if (options.Status == "inactive")
                    query = query.Where(u => !u.IsActivated);
                else if (options.Status == "pending")
                    query = query.Where(u => !u.EmailConfirmed);
            }

            var totalUsers = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip((options.Page - 1) * options.PageSize)
                .Take(options.PageSize)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber ?? "",
                    RegisteredDate = u.CreatedAt,
                    BookingCount = u.Bookings.Count,
                    TotalSpent = u.Bookings.Sum(b => (decimal?)b.TotalPrice) ?? 0,
                    IsActivated = u.IsActivated,
                    EmailConfirmed = u.EmailConfirmed
                })
                .ToListAsync();

            return new UsersViewModel
            {
                Users = users,
                CurrentPage = options.Page,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)options.PageSize),
                TotalUsers = totalUsers,
                Query = options,
                ActiveUsers = activeUsers,
                NewUsersLast30Days = newUsers,
                BookingUsers = bookingUsers,
                InactiveUsers = inactiveUsers
            };
        }

        public async Task<IdentityResult> Add(CreateUserViewModel model, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var phoneExists = await _context.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber, ct);
                if (phoneExists)
                {
                    var error = IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicatePhoneNumber",
                        Description = "Số điện thoại đã được sử dụng."
                    });
                    return error;
                }
            }

            var user = new ApplicationUser
            {
                UserName = model.Email, // use email as username
                Email = model.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber,
                FullName = model.FullName,
                IsActivated = model.IsActivated,
                EmailConfirmed = model.EmailConfirmed,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            var roleResult = await _userManager.AddToRoleAsync(user, "GUEST");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return roleResult;
            }
            return result;
        }

        public async Task<UserDetailsViewModel?> GetById(string id, CancellationToken ct = default)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user == null) return null;

            return new UserDetailsViewModel
            {
                User = new EditUserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    IsActivated = user.IsActivated,
                    EmailConfirmed = user.EmailConfirmed
                }
            };
        }

        public async Task<IdentityResult> Update(EditUserViewModel model, CancellationToken ct = default)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.Id, ct);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email; // keep Email as username
            user.PhoneNumber = model.PhoneNumber;
            user.IsActivated = model.IsActivated;
            user.EmailConfirmed = model.EmailConfirmed;

            return await _userManager.UpdateAsync(user);
        }

    }
}
