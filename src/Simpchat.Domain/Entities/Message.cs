using Simpchat.Domain.Common;
using static System.Net.Mime.MediaTypeNames;

namespace Simpchat.Domain.Entities
{
    public class Message : BaseEntity
    {
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? ReplyId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ChatId { get; set; }
        public User Sender { get; set; }
        public Chat Chat { get; set; }
        public Message ReplyTo { get; set; }
        public ICollection<MessageReaction> Reactions { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Message> Replies { get; set; }
    }
}
