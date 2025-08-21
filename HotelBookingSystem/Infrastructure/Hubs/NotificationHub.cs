using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HotelBookingSystem.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = Context.User?.IsInRole("Admin") ?? false;
                var userName = Context.User?.Identity?.Name ?? "Anonymous";

                if (!string.IsNullOrEmpty(userId))
                {
                    // Join user-specific group
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

                    // Join admin group if user is admin
                    if (isAdmin)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
                    }
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] OnConnectedAsync failed: {ex.Message}");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = Context.User?.IsInRole("Admin") ?? false;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");

                if (isAdmin)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminGroup");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Send notification to specific user
        public async Task SendToUser(string userId, string message, string type = "info")
        {
            await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
            {
                message,
                type,
                timestamp = DateTime.Now,
                sender = Context.User?.Identity?.Name
            });
        }

        // Send notification to all admins
        public async Task SendToAdmins(string message, string type = "info", object? data = null)
        {
            await Clients.Group("AdminGroup").SendAsync("ReceiveAdminNotification", new
            {
                message,
                type,
                timestamp = DateTime.Now,
                data,
                sender = Context.User?.Identity?.Name
            });
        }
    }
}
