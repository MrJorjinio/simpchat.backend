using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Common.Repository;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public NotificationRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(Notification entity)
        {
            await _dbContext.Notifications.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task DeleteAsync(Notification entity)
        {
            _dbContext.Notifications.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Notification>?> GetAllAsync()
        {
            return await _dbContext.Notifications
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Notifications
                .Include(n => n.Message)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<bool> CheckIsNotSeenAsync(Guid messageId, Guid userId)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => n.MessageId == messageId && n.ReceiverId == userId && n.IsSeen == true)
                .ToListAsync();

            return notifications.Count == 0;
        }

        public async Task<bool> GetMessageSeenStatusAsync(Guid messageId)
        {
            var seenNotification = await _dbContext.Notifications
                .Where(n => n.MessageId == messageId && n.IsSeen == true)
                .ToListAsync();

            if (seenNotification.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task<int> GetUserChatNotificationsCountAsync(Guid userId, Guid chatId)
        {
            return _dbContext.Notifications
                .Include(n => n.Message)
                .Where(n => n.ReceiverId == userId && n.Message.ChatId == chatId && n.IsSeen == false)
                .CountAsync();
        }

        public async Task UpdateAsync(Notification entity)
        {
            _dbContext.Notifications.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid?> GetIdAsync(Guid messageId, Guid userId)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.MessageId == messageId && n.ReceiverId == userId);

            return notification?.Id;
        }

        public async Task<List<Notification>?> GetMultipleByIdsAsync(List<Guid> notificationIds)
        {
            return await _dbContext.Notifications
                .Where(n => notificationIds.Contains(n.Id))
                .ToListAsync();
        }

        public async Task UpdateMultipleAsync(List<Notification> notifications)
        {
            _dbContext.Notifications.UpdateRange(notifications);
            await _dbContext.SaveChangesAsync();
        }
    }
}
