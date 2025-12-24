namespace Simpchat.Application.Interfaces.Services
{
    /// <summary>
    /// Interface for sending real-time notifications via SignalR hub.
    /// This allows application services to send hub notifications without depending on SignalR directly.
    /// </summary>
    public interface IChatHubService
    {
        /// <summary>
        /// Notify a user that they have been added to a chat (group or channel).
        /// </summary>
        Task NotifyUserAddedToChatAsync(Guid userId, Guid chatId, string chatName, string chatType, string? chatAvatarUrl);

        /// <summary>
        /// Notify a user that a new conversation has been created with them.
        /// </summary>
        Task NotifyNewConversationAsync(Guid receiverId, Guid conversationId, Guid senderId, string senderUsername, string? senderAvatarUrl);

        /// <summary>
        /// Notify the sender that a conversation was created (so they can switch from temp chat to real chat).
        /// </summary>
        Task NotifyConversationCreatedAsync(Guid senderId, Guid conversationId, Guid receiverId, string receiverUsername, string? receiverAvatarUrl);

        /// <summary>
        /// Add a user's connection to a chat group for real-time updates.
        /// </summary>
        Task AddUserToChatGroupAsync(Guid userId, Guid chatId);

        /// <summary>
        /// Remove a user's connection from a chat group.
        /// </summary>
        Task RemoveUserFromChatGroupAsync(Guid userId, Guid chatId);

        /// <summary>
        /// Broadcast user online status to related users.
        /// </summary>
        Task BroadcastUserOnlineAsync(Guid userId);

        /// <summary>
        /// Broadcast user offline status to related users.
        /// </summary>
        Task BroadcastUserOfflineAsync(Guid userId);
    }
}
