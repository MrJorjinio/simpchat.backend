using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Messages;
using Simpchat.Application.Models.Presence;
using System.Security.Claims;

namespace Simpchat.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IPresenceService _presenceService;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IMessageReactionService _messageReactionService;
        private readonly INotificationService _notificationService;
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IPresenceService presenceService,
            IUserService userService,
            IMessageService messageService,
            IMessageReactionService messageReactionService,
            INotificationService notificationService,
            IChatService chatService,
            ILogger<ChatHub> logger)
        {
            _presenceService = presenceService;
            _userService = userService;
            _messageService = messageService;
            _messageReactionService = messageReactionService;
            _notificationService = notificationService;
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                await base.OnConnectedAsync();
                return;
            }

            // Track connection
            await _presenceService.UserConnectedAsync(userId, Context.ConnectionId);

            // Auto-join user to all their chat groups for real-time messaging
            var userChats = await _chatService.GetUserChatsAsync(userId);
            if (userChats.IsSuccess && userChats.Value != null)
            {
                foreach (var chat in userChats.Value)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chat.Id}");
                    _logger.LogInformation("User {UserId} auto-joined chat group: chat_{ChatId}", userId, chat.Id);
                }
            }

            // Broadcast to related users
            await BroadcastUserOnlineAsync(userId);

            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            // Remove connection
            await _presenceService.UserDisconnectedAsync(userId, Context.ConnectionId);

            // If user has no more connections, update LastSeen
            if (!_presenceService.IsUserOnline(userId))
            {
                await _userService.SetLastSeenAsync(userId);
                await BroadcastUserOfflineAsync(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ===== MESSAGING METHODS =====

        /// <summary>
        /// Send a text message to a chat
        /// </summary>
        public async Task SendMessage(SendMessageRequest request)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                // Get sender details ONCE and reuse
                var senderResult = await _userService.GetByIdAsync(userId, userId);
                var senderUsername = senderResult.IsSuccess ? senderResult.Value.Username : "Unknown";
                var senderAvatarUrl = senderResult.IsSuccess ? senderResult.Value.AvatarUrl : null;

                var messageDto = new PostMessageDto
                {
                    ChatId = request.ChatId,
                    Content = request.Content,
                    ReceiverId = request.ReceiverId,
                    ReplyId = request.ReplyId,
                    SenderId = userId
                };

                var result = await _messageService.SendMessageAsync(messageDto, null);

                if (result.IsSuccess)
                {
                    // Broadcast to chat participants (sender info already fetched above)
                    await Clients.Group($"chat_{request.ChatId}").SendAsync("ReceiveMessage", new
                    {
                        messageId = result.Value,
                        chatId = request.ChatId,
                        senderId = userId,
                        senderUsername = senderUsername,
                        senderAvatarUrl = senderAvatarUrl,
                        content = request.Content,
                        fileUrl = (string?)null,
                        replyId = request.ReplyId,
                        sentAt = DateTimeOffset.UtcNow,
                        isSeen = false,
                        seenAt = (DateTimeOffset?)null
                    });

                    // Broadcast notification to recipients (pass sender info to avoid re-fetching)
                    await BroadcastNewNotificationAsync(userId, senderUsername, senderAvatarUrl, request, result.Value);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", new { error = result.Error?.Message ?? "Failed to send message" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", new { error = "Failed to send message" });
            }
        }

        /// <summary>
        /// Edit an existing message
        /// </summary>
        public async Task EditMessage(Guid chatId, Guid messageId, string content)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                var updateDto = new UpdateMessageDto { Content = content };
                var result = await _messageService.UpdateAsync(messageId, updateDto, null, userId);

                if (result.IsSuccess)
                {
                    await Clients.Group($"chat_{chatId}").SendAsync("MessageEdited", new
                    {
                        messageId,
                        chatId,
                        content,
                        editedAt = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", new { error = result.Error?.Message ?? "Failed to edit message" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message");
                await Clients.Caller.SendAsync("Error", new { error = "Failed to edit message" });
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        public async Task DeleteMessage(Guid chatId, Guid messageId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                var result = await _messageService.DeleteAsync(messageId, userId);

                if (result.IsSuccess)
                {
                    await Clients.Group($"chat_{chatId}").SendAsync("MessageDeleted", new
                    {
                        messageId,
                        chatId,
                        deletedAt = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", new { error = result.Error?.Message ?? "Failed to delete message" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                await Clients.Caller.SendAsync("Error", new { error = "Failed to delete message" });
            }
        }

        // ===== REACTION METHODS =====

        /// <summary>
        /// Toggle a reaction on a message (add if not exists, remove if exists)
        /// </summary>
        /// <param name="chatId">The chat ID</param>
        /// <param name="messageId">The message ID</param>
        /// <param name="reactionType">One of: Like, Love, Laugh, Sad, Angry</param>
        public async Task ToggleReaction(Guid chatId, Guid messageId, string reactionType)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                // Parse the reaction type
                if (!Enum.TryParse<Domain.Enums.ReactionType>(reactionType, ignoreCase: true, out var parsedReactionType))
                {
                    await Clients.Caller.SendAsync("Error", new { error = $"Invalid reaction type. Valid types are: {string.Join(", ", Enum.GetNames<Domain.Enums.ReactionType>())}" });
                    return;
                }

                var result = await _messageReactionService.ToggleReactionAsync(messageId, parsedReactionType, userId);

                if (result.IsSuccess)
                {
                    var wasAdded = result.Value;
                    await Clients.Group($"chat_{chatId}").SendAsync(wasAdded ? "ReactionAdded" : "ReactionRemoved", new
                    {
                        messageId,
                        reactionType,
                        userId,
                        chatId
                    });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", new { error = result.Error?.Message ?? "Failed to toggle reaction" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling reaction");
                await Clients.Caller.SendAsync("Error", new { error = "Failed to toggle reaction" });
            }
        }

        // ===== NOTIFICATION METHODS =====

        /// <summary>
        /// Mark notification(s) as seen
        /// </summary>
        public async Task MarkAsSeen(List<Guid> notificationIds)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                var result = await _notificationService.SetMultipleAsSeenAsync(notificationIds);

                if (result.IsSuccess)
                {
                    await Clients.Caller.SendAsync("NotificationsMarkedSeen", new { notificationIds });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", new { error = result.Error?.Message ?? "Failed to mark as seen" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as seen");
                await Clients.Caller.SendAsync("Error", new { error = "Failed to mark as seen" });
            }
        }

        // ===== TYPING INDICATOR =====

        /// <summary>
        /// Notify chat participants that user is typing
        /// </summary>
        public async Task Typing(Guid chatId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                await Clients.OthersInGroup($"chat_{chatId}").SendAsync("UserTyping", new
                {
                    userId,
                    chatId,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator");
            }
        }

        /// <summary>
        /// Notify chat participants that user stopped typing
        /// </summary>
        public async Task StopTyping(Guid chatId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                await Clients.OthersInGroup($"chat_{chatId}").SendAsync("UserStoppedTyping", new
                {
                    userId,
                    chatId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending stop typing indicator");
            }
        }

        // ===== READ RECEIPTS =====

        /// <summary>
        /// Mark all messages in a chat as seen by the current user
        /// </summary>
        public async Task MarkMessagesAsSeen(Guid chatId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                var result = await _messageService.MarkMessagesAsSeenAsync(chatId, userId);

                if (result.IsSuccess && result.Value.Any())
                {
                    // Broadcast to all chat participants that these messages are now seen
                    await Clients.Group($"chat_{chatId}").SendAsync("MessagesMarkedSeen", new
                    {
                        chatId,
                        messageIds = result.Value,
                        seenByUserId = userId,
                        seenAt = DateTimeOffset.UtcNow
                    });

                    _logger.LogInformation("User {UserId} marked {Count} messages as seen in chat {ChatId}",
                        userId, result.Value.Count, chatId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as seen");
                await Clients.Caller.SendAsync("Error", new { error = "Failed to mark messages as seen" });
            }
        }

        // ===== CHAT ROOM MANAGEMENT =====

        /// <summary>
        /// Join a chat room to receive real-time updates
        /// </summary>
        public async Task JoinChat(Guid chatId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty) return;

                // Verify user has access to this chat
                var chatResult = await _chatService.GetByIdAsync(chatId, userId);
                if (chatResult.IsSuccess)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
                    _logger.LogInformation("User {UserId} joined chat {ChatId}", userId, chatId);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", new { error = "Access denied to chat" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat");
                await Clients.Caller.SendAsync("Error", new { error = "Failed to join chat" });
            }
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        public async Task LeaveChat(Guid chatId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
                _logger.LogInformation("User {UserId} left chat {ChatId}", GetUserId(), chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat");
            }
        }

        public async Task<Dictionary<string, bool>> GetPresenceStates(List<string> userIds)
        {
            try
            {
                var presenceStates = new Dictionary<string, bool>();

                foreach (var userIdString in userIds)
                {
                    if (Guid.TryParse(userIdString, out var userId))
                    {
                        presenceStates[userIdString] = _presenceService.IsUserOnline(userId);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid userId format: {UserId}", userIdString);
                        presenceStates[userIdString] = false;
                    }
                }

                _logger.LogInformation("Fetched presence states for {Count} users", userIds.Count);
                return await Task.FromResult(presenceStates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching presence states");
                return new Dictionary<string, bool>();
            }
        }

        // ===== PRIVATE HELPER METHODS =====

        private async Task BroadcastUserOnlineAsync(Guid userId)
        {
            var relatedUserIds = await _presenceService.GetRelatedUserIdsAsync(userId);
            var statusDto = new UserStatusDto
            {
                UserId = userId,
                IsOnline = true,
                LastSeen = null
            };

            await SendToUsersAsync(relatedUserIds, "UserOnline", statusDto);
        }

        private async Task BroadcastUserOfflineAsync(Guid userId)
        {
            var relatedUserIds = await _presenceService.GetRelatedUserIdsAsync(userId);

            // Get updated user with LastSeen
            var userResult = await _userService.GetByIdAsync(userId, userId);
            var lastSeen = userResult.IsSuccess ? userResult.Value.LastSeen : DateTimeOffset.UtcNow;

            var statusDto = new UserStatusDto
            {
                UserId = userId,
                IsOnline = false,
                LastSeen = lastSeen
            };

            await SendToUsersAsync(relatedUserIds, "UserOffline", statusDto);
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
                        await Clients.Client(connectionId).SendAsync(methodName, data);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send {Method} to connection {ConnectionId}",
                            methodName, connectionId);
                    }
                }
            }
        }

        private async Task BroadcastNewNotificationAsync(Guid senderId, string senderUsername, string? senderAvatarUrl, SendMessageRequest request, Guid messageId)
        {
            try
            {
                var recipientIds = new List<Guid>();
                string chatName = "";
                string chatAvatar = "";

                if (request.ReceiverId.HasValue && request.ReceiverId.Value != Guid.Empty)
                {
                    // Direct conversation - use sender info as chat name/avatar
                    recipientIds.Add(request.ReceiverId.Value);
                    chatName = senderUsername;
                    chatAvatar = senderAvatarUrl ?? "";
                }
                else if (request.ChatId.HasValue)
                {
                    // Group or channel chat - use lightweight query instead of heavy GetProfileAsync
                    var chatResult = await _chatService.GetBasicInfoAsync(request.ChatId.Value);
                    if (chatResult.IsSuccess)
                    {
                        chatName = chatResult.Value.Name;
                        chatAvatar = chatResult.Value.AvatarUrl ?? "";
                        recipientIds = chatResult.Value.MemberIds
                            .Where(id => id != senderId)
                            .ToList();
                    }
                }

                if (recipientIds.Any())
                {
                    var notificationPayload = new
                    {
                        messageId,
                        chatId = request.ChatId ?? Guid.Empty,
                        chatName,
                        chatAvatar,
                        senderName = senderUsername,
                        content = request.Content,
                        fileUrl = (string?)null,
                        sentTime = DateTimeOffset.UtcNow
                    };

                    // Broadcast to each recipient's connections
                    foreach (var recipientId in recipientIds)
                    {
                        var connections = _presenceService.GetUserConnections(recipientId);
                        foreach (var connectionId in connections)
                        {
                            try
                            {
                                await Clients.Client(connectionId).SendAsync("NewNotification", notificationPayload);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send notification to connection {ConnectionId}", connectionId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notifications");
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    // ===== REQUEST MODELS =====

    public class SendMessageRequest
    {
        public Guid? ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid? ReceiverId { get; set; }
        public Guid? ReplyId { get; set; }
    }
}
