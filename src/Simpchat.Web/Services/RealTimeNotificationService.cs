using Microsoft.AspNetCore.SignalR;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Web.Hubs;

namespace Simpchat.Web.Services
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IPresenceService _presenceService;
        private readonly ILogger<RealTimeNotificationService> _logger;

        public RealTimeNotificationService(
            IHubContext<ChatHub> hubContext,
            IPresenceService presenceService,
            ILogger<RealTimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _presenceService = presenceService;
            _logger = logger;
        }

        // Permission events
        public async Task NotifyPermissionGrantedAsync(Guid chatId, Guid userId, string permissionName)
        {
            await SendToUserAsync(userId, "PermissionGranted", new
            {
                chatId,
                userId,
                permissionName,
                timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Notified user {UserId} of permission {Permission} granted for chat {ChatId}",
                userId, permissionName, chatId);
        }

        public async Task NotifyPermissionRevokedAsync(Guid chatId, Guid userId, string permissionName)
        {
            await SendToUserAsync(userId, "PermissionRevoked", new
            {
                chatId,
                userId,
                permissionName,
                timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Notified user {UserId} of permission {Permission} revoked for chat {ChatId}",
                userId, permissionName, chatId);
        }

        public async Task NotifyAllPermissionsRevokedAsync(Guid chatId, Guid userId)
        {
            await SendToUserAsync(userId, "AllPermissionsRevoked", new
            {
                chatId,
                userId,
                timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Notified user {UserId} of all permissions revoked for chat {ChatId}",
                userId, chatId);
        }

        // Block events
        public async Task NotifyUserBlockedAsync(Guid blockerId, Guid blockedUserId)
        {
            // Notify the blocked user that they've been blocked
            await SendToUserAsync(blockedUserId, "UserBlockedYou", new
            {
                blockerId,
                timestamp = DateTimeOffset.UtcNow
            });

            // Also notify the blocker for UI update
            await SendToUserAsync(blockerId, "YouBlockedUser", new
            {
                blockedUserId,
                timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("User {BlockerId} blocked user {BlockedUserId}", blockerId, blockedUserId);
        }

        public async Task NotifyUserUnblockedAsync(Guid unblockerId, Guid unblockedUserId)
        {
            // Notify the unblocked user
            await SendToUserAsync(unblockedUserId, "UserUnblockedYou", new
            {
                unblockerId,
                timestamp = DateTimeOffset.UtcNow
            });

            // Also notify the unblocker for UI update
            await SendToUserAsync(unblockerId, "YouUnblockedUser", new
            {
                unblockedUserId,
                timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("User {UnblockerId} unblocked user {UnblockedUserId}", unblockerId, unblockedUserId);
        }

        // Chat deletion events
        public async Task NotifyChatDeletedAsync(Guid chatId, List<Guid> memberIds)
        {
            foreach (var memberId in memberIds)
            {
                await SendToUserAsync(memberId, "ChatDeleted", new
                {
                    chatId,
                    timestamp = DateTimeOffset.UtcNow
                });
            }

            _logger.LogInformation("Notified {Count} members of chat {ChatId} deletion", memberIds.Count, chatId);
        }

        public async Task NotifyUserRemovedFromChatAsync(Guid chatId, Guid userId)
        {
            await SendToUserAsync(userId, "RemovedFromChat", new
            {
                chatId,
                timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Notified user {UserId} of removal from chat {ChatId}", userId, chatId);
        }

        private async Task SendToUserAsync(Guid userId, string methodName, object data)
        {
            var connections = _presenceService.GetUserConnections(userId);
            foreach (var connectionId in connections)
            {
                try
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync(methodName, data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send {Method} to connection {ConnectionId}",
                        methodName, connectionId);
                }
            }
        }
    }
}
