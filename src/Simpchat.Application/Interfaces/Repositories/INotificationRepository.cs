using Simpchat.Application.Common.Repository;
using Simpchat.Domain.Entities;

namespace Simpchat.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<int> GetUserChatNotificationsCountAsync(Guid userId, Guid chatId);
        Task<bool> GetMessageSeenStatusAsync(Guid messageId);
        Task<bool> CheckIsNotSeenAsync(Guid messageId, Guid userId);
        Task<Guid?> GetIdAsync(Guid messageId, Guid userId);
        Task<List<Notification>?> GetMultipleByIdsAsync(List<Guid> notificationIds);
        Task UpdateMultipleAsync(List<Notification> notifications);
    }
}
