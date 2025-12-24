using Simpchat.Domain.Common;

namespace Simpchat.Domain.Entities
{
    public class ChatBan : BaseEntity
    {
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }
        public DateTime BannedAt { get; set; } = DateTime.UtcNow;
        public Chat Chat { get; set; }
        public User User { get; set; }
    }
}
