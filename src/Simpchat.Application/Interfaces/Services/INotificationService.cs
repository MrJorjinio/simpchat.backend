using Simpchat.Application.Models.ApiResult;
using Simpchat.Application.Models.Notifications;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<Result<List<GetAllUserNotificationDto>>> GetAllUserNotificationsAsync(Guid userId);
        Task<Result> SetAsSeenAsync(Guid notificationId);
        Task<Result> SetMultipleAsSeenAsync(List<Guid> notificationIds);
    }
}
