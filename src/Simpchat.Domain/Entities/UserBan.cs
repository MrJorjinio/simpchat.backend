using Simpchat.Domain.Common;

namespace Simpchat.Domain.Entities
{
    /// <summary>
    /// Represents a user-to-user ban. When a user bans another user,
    /// the banned user cannot start conversations or send messages to the banning user.
    /// </summary>
    public class UserBan : BaseEntity
    {
        /// <summary>
        /// The user who initiated the ban (the one who blocked the other user)
        /// </summary>
        public Guid BlockerId { get; set; }

        /// <summary>
        /// The user who is banned/blocked
        /// </summary>
        public Guid BlockedUserId { get; set; }

        /// <summary>
        /// When the ban was created
        /// </summary>
        public DateTime BannedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property - the user who initiated the ban
        /// </summary>
        public User Blocker { get; set; }

        /// <summary>
        /// Navigation property - the user who is banned
        /// </summary>
        public User BlockedUser { get; set; }
    }
}
