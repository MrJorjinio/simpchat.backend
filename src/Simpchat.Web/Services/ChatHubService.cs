using Microsoft.AspNetCore.SignalR;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Web.Hubs;

namespace Simpchat.Web.Services
{
    /// <summary>
    /// Implementation of IChatHubService that uses SignalR's IHubContext to send notifications.
    /// </summary>
    public class ChatHubService : IChatHubService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IPresenceService _presenceService;
        private readonly ILogger<ChatHubService> _logger;

        public ChatHubService(
            IHubContext<ChatHub> hubContext,
            IPresenceService presenceService,
            ILogger<ChatHubService> logger)
        {
            _hubContext = hubContext;
            _presenceService = presenceService;
            _logger = logger;
        }

        public async Task NotifyUserAddedToChatAsync(Guid userId, Guid chatId, string chatName, string chatType, string? chatAvatarUrl)
        {
            var connections = _presenceService.GetUserConnections(userId);

            if (!connections.Any())
            {
                _logger.LogInformation("User {UserId} is offline, skipping AddedToChat notification for chat {ChatId}", userId, chatId);
                return;
            }

            var payload = new
            {
                chatId = chatId.ToString(),
                chatName,
                chatType,
                chatAvatarUrl,
                addedAt = DateTimeOffset.UtcNow
            };

            foreach (var connectionId in connections)
            {
                try
                {
                    // Add the user's connection to the chat group so they receive future messages
                    await _hubContext.Groups.AddToGroupAsync(connectionId, $"chat_{chatId}");

                    // Send the notification
                    await _hubContext.Clients.Client(connectionId).SendAsync("AddedToChat", payload);
                    _logger.LogInformation("Notified user {UserId} about being added to chat {ChatId}", userId, chatId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to notify user {UserId} about being added to chat {ChatId}", userId, chatId);
                }
            }
        }

        public async Task NotifyNewConversationAsync(Guid receiverId, Guid conversationId, Guid senderId, string senderUsername, string? senderAvatarUrl)
        {
            var connections = _presenceService.GetUserConnections(receiverId);

            if (!connections.Any())
            {
                _logger.LogInformation("User {UserId} is offline, skipping NewConversation notification", receiverId);
                return;
            }

            var payload = new
            {
                conversationId = conversationId.ToString(),
                senderId = senderId.ToString(),
                senderUsername,
                senderAvatarUrl,
                createdAt = DateTimeOffset.UtcNow
            };

            foreach (var connectionId in connections)
            {
                try
                {
                    // Add the user's connection to the conversation group
                    await _hubContext.Groups.AddToGroupAsync(connectionId, $"chat_{conversationId}");

                    // Send the notification
                    await _hubContext.Clients.Client(connectionId).SendAsync("NewConversation", payload);
                    _logger.LogInformation("Notified user {UserId} about new conversation {ConversationId} from {SenderId}",
                        receiverId, conversationId, senderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to notify user {UserId} about new conversation", receiverId);
                }
            }
        }

        public async Task NotifyConversationCreatedAsync(Guid senderId, Guid conversationId, Guid receiverId, string receiverUsername, string? receiverAvatarUrl)
        {
            var connections = _presenceService.GetUserConnections(senderId);

            if (!connections.Any())
            {
                _logger.LogInformation("Sender {UserId} is offline, skipping ConversationCreated notification", senderId);
                return;
            }

            var payload = new
            {
                conversationId = conversationId.ToString(),
                receiverId = receiverId.ToString(),
                receiverUsername,
                receiverAvatarUrl,
                createdAt = DateTimeOffset.UtcNow
            };

            foreach (var connectionId in connections)
            {
                try
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ConversationCreated", payload);
                    _logger.LogInformation("Notified sender {UserId} about conversation {ConversationId} creation",
                        senderId, conversationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to notify sender {UserId} about conversation creation", senderId);
                }
            }
        }

        public async Task AddUserToChatGroupAsync(Guid userId, Guid chatId)
        {
            var connections = _presenceService.GetUserConnections(userId);

            foreach (var connectionId in connections)
            {
                try
                {
                    await _hubContext.Groups.AddToGroupAsync(connectionId, $"chat_{chatId}");
                    _logger.LogInformation("Added user {UserId} connection {ConnectionId} to chat group {ChatId}",
                        userId, connectionId, chatId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add user {UserId} to chat group {ChatId}", userId, chatId);
                }
            }
        }

        public async Task RemoveUserFromChatGroupAsync(Guid userId, Guid chatId)
        {
            var connections = _presenceService.GetUserConnections(userId);

            foreach (var connectionId in connections)
            {
                try
                {
                    await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"chat_{chatId}");
                    _logger.LogInformation("Removed user {UserId} connection {ConnectionId} from chat group {ChatId}",
                        userId, connectionId, chatId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove user {UserId} from chat group {ChatId}", userId, chatId);
                }
            }
        }

        public async Task BroadcastUserOnlineAsync(Guid userId)
        {
            var relatedUserIds = await _presenceService.GetRelatedUserIdsAsync(userId);

            var payload = new
            {
                userId = userId.ToString(),
                isOnline = true,
                lastSeen = (DateTimeOffset?)null
            };

            await SendToUsersAsync(relatedUserIds, "UserOnline", payload);
            _logger.LogInformation("Broadcast online status for user {UserId} to {Count} related users",
                userId, relatedUserIds.Count);
        }

        public async Task BroadcastUserOfflineAsync(Guid userId)
        {
            var relatedUserIds = await _presenceService.GetRelatedUserIdsAsync(userId);

            var payload = new
            {
                userId = userId.ToString(),
                isOnline = false,
                lastSeen = DateTimeOffset.UtcNow
            };

            await SendToUsersAsync(relatedUserIds, "UserOffline", payload);
            _logger.LogInformation("Broadcast offline status for user {UserId} to {Count} related users",
                userId, relatedUserIds.Count);
        }

        private async Task SendToUsersAsync(List<Guid> userIds, string methodName, object data)
        {
            foreach (var userId in userIds)
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
}
