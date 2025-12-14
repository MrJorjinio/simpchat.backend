using Simpchat.Application.Errors;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.ApiResult;
using Simpchat.Application.Models.Notifications;
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

        public async Task<Result<List<GetAllUserNotificationDto>>> GetAllUserNotificationsAsync(Guid userId)
        {
            var notifications = await _notificationRepo.GetUserNotificationsAsync(userId);

            if (notifications == null || notifications.Count == 0)
            {
                return Result.Success(new List<GetAllUserNotificationDto>());
            }

            var notificationDtos = notifications
                .Where(n => n.Message != null && n.Message.Chat != null && n.Message.Sender != null)
                .Select(n =>
                {
                    var message = n.Message;
                    var chat = message.Chat;
                    var sender = message.Sender;

                    var chatName = "";
                    var chatAvatar = "";

                    if (chat.Group != null)
                    {
                        chatName = chat.Group.Name ?? "";
                        chatAvatar = chat.Group.AvatarUrl ?? "";
                    }
                    else if (chat.Conversation != null)
                    {
                        var otherUser = chat.Conversation.UserId1 == userId
                            ? chat.Conversation.User2
                            : chat.Conversation.User1;
                        if (otherUser != null)
                        {
                            chatName = otherUser.Username ?? "";
                            chatAvatar = otherUser.AvatarUrl ?? "";
                        }
                    }

                    return new GetAllUserNotificationDto
                    {
                        NotificationId = n.Id,
                        ChatId = chat.Id,
                        MessageId = message.Id,
                        ChatName = chatName,
                        ChatAvatar = chatAvatar,
                        SenderName = sender.Username ?? "",
                        Content = message.Content ?? "",
                        FileUrl = message.FileUrl,
                        SentTime = message.SentAt
                    };
                }).ToList();

            return Result.Success(notificationDtos);
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
                return Result.Success();
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
