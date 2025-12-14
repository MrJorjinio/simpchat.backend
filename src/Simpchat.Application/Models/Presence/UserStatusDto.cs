namespace Simpchat.Application.Models.Presence
{
    public class UserStatusDto
    {
        public Guid UserId { get; set; }
        public bool IsOnline { get; set; }
        public DateTimeOffset? LastSeen { get; set; }
    }
}
