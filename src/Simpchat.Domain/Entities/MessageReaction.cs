using Simpchat.Domain.Common;
using Simpchat.Domain.Enums;

namespace Simpchat.Domain.Entities
{
    public class MessageReaction : BaseEntity
    {
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid UserId { get; set; }
        public Guid MessageId { get; set; }
        public ReactionType ReactionType { get; set; }
        public User User { get; set; }
        public Message Message { get; set; }
    }
}
