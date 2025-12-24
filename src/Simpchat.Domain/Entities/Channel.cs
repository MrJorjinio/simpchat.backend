using Simpchat.Domain.Common;

namespace Simpchat.Domain.Entities
{
    public class Channel : BaseEntity
    {
        public string AvatarUrl { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid CreatedById { get; set; }
        public Chat Chat { get; set; }
        public User Owner { get; set; }
        public ICollection<ChannelSubscriber> Subscribers { get; set; }
    }
}
