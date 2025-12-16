using Simpchat.Domain.Common;

namespace Simpchat.Domain.Entities
{
    public class Group : BaseEntity
    {
        public string AvatarUrl { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid CreatedById { get; set; }
        public Chat Chat { get; set; }
        public User Owner { get; set; }
        public ICollection<GroupMember> Members { get; set; }
    }
}
