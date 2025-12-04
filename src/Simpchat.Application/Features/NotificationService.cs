using Simpchat.Application.Errors;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.ApiResult;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Features
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;

        public NotificationService(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<Result> SetAsSeenAsync(Guid notificationId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);

            if (notification is null)
            {
                return Result.Failure(ApplicationErrors.Notification.IdNotFound);
            }

            notification.IsSeen = true;

            await _notificationRepo.UpdateAsync(notification);

            return Result.Success();
        }

        public async Task<Result> SetMultipleAsSeenAsync(List<Guid> notificationIds)
        {
            if (notificationIds == null || notificationIds.Count == 0)
            {
                return Result.Success(); // No notifications to mark
            }

            var notifications = await _notificationRepo.GetMultipleByIdsAsync(notificationIds);

            if (notifications == null || notifications.Count == 0)
            {
                return Result.Failure(ApplicationErrors.Notification.IdNotFound);
            }

            foreach (var notification in notifications)
            {
                notification.IsSeen = true;
            }

            await _notificationRepo.UpdateMultipleAsync(notifications);

            return Result.Success();
        }
    }
}
