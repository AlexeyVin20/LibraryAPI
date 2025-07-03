using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using LibraryAPI.Models;
using System.Security.Claims;

namespace LibraryAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Отправка подтверждения о получении уведомления
        public async Task ConfirmNotificationReceived(string notificationId)
        {
            // Логика подтверждения получения уведомления
            await Clients.Caller.SendAsync("NotificationConfirmed", notificationId);
        }

        // Отправка статуса "прочитано"
        public async Task MarkNotificationAsRead(string notificationId)
        {
            // Логика отметки уведомления как прочитанного
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }
    }
} 