using Simpchat.Application.Models.Reactions;

namespace Simpchat.Application.Models.Messages
{
    public class PinnedMessageDto
    {
        public Guid MessageId { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderAvatarUrl { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public DateTimeOffset? PinnedAt { get; set; }
        public Guid? PinnedById { get; set; }
        public string? PinnedByUsername { get; set; }
        public List<MessageReactionDto> MessageReactions { get; set; } = new();
    }
}
