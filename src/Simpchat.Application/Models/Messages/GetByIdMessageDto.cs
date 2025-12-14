using Simpchat.Application.Models.Reactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Messages
{
    public class GetByIdMessageDto
    {
        public Guid MessageId { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; }
        public string? SenderAvatarUrl { get; set; }
        public Guid? ReplyId { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public bool IsSeen { get; set; }
        public bool IsNotificated { get; set; }
        public Guid NotificationId { get; set; }
        public List<GetAllMessageReaction> MessageReactions { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
