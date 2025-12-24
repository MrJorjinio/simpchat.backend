namespace Simpchat.Application.Interfaces.Services
{
    public interface IRealTimeNotificationService
    {
        // Permission events
        Task NotifyPermissionGrantedAsync(Guid chatId, Guid userId, string permissionName);
        Task NotifyPermissionRevokedAsync(Guid chatId, Guid userId, string permissionName);
        Task NotifyAllPermissionsRevokedAsync(Guid chatId, Guid userId);

        // Block events
        Task NotifyUserBlockedAsync(Guid blockerId, Guid blockedUserId);
        Task NotifyUserUnblockedAsync(Guid unblockerId, Guid unblockedUserId);

        // Chat deletion events
        Task NotifyChatDeletedAsync(Guid chatId, List<Guid> memberIds);
        Task NotifyUserRemovedFromChatAsync(Guid chatId, Guid userId);
    }
}
