using Simpchat.Application.Common.Repository;
using Simpchat.Domain.Entities;

namespace Simpchat.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task CreateBatchAsync(List<Notification> notifications);
        Task<int> GetUserChatNotificationsCountAsync(Guid userId, Guid chatId);
        Task<bool> GetMessageSeenStatusAsync(Guid messageId);
        Task<bool> CheckIsNotSeenAsync(Guid messageId, Guid userId);
        Task<Guid?> GetIdAsync(Guid messageId, Guid userId);
        Task<List<Guid>> GetIdsByMessageIdsAsync(List<Guid> messageIds, Guid userId);
        Task MarkSeenByMessageIdsAsync(List<Guid> messageIds, Guid userId);
        /// <summary>
        /// Returns a map of MessageId -> NotificationId for the given messages and user
        /// </summary>
        Task<Dictionary<Guid, Guid>> GetNotificationMapByMessageIdsAsync(List<Guid> messageIds, Guid userId);
        /// <summary>
        /// Returns set of message IDs that have unseen notifications for this user
        /// </summary>
        Task<HashSet<Guid>> GetUnseenMessageIdsAsync(List<Guid> messageIds, Guid userId);
        Task<List<Notification>?> GetMultipleByIdsAsync(List<Guid> notificationIds);
        Task UpdateMultipleAsync(List<Notification> notifications);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
    }
}
