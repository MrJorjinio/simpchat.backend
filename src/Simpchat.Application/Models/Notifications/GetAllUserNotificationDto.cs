using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Notifications
{
    public class GetAllUserNotificationDto
    {
        public Guid NotificationId { get; set; }
        public Guid ChatId { get; set; }
        public Guid MessageId { get; set; }
        public string ChatName { get; set; }
        public string ChatAvatar { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public string? FileUrl { get; set; }
        public DateTimeOffset SentTime { get; set; }
    }
}
