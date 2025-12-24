using Simpchat.Application.Models.Reactions;

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
        public DateTimeOffset? SeenAt { get; set; }
        public bool IsNotificated { get; set; }
        public Guid NotificationId { get; set; }
        public List<MessageReactionDto> MessageReactions { get; set; } = new();
        public bool IsCurrentUser { get; set; }
    }
}
