using Simpchat.Application.Models.Users;
using System;

namespace Simpchat.Application.Models.Chats
{
    public class ChatMemberDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserResponseDto User { get; set; }
        public string JoinedAt { get; set; }
        public string Role { get; set; } // "admin", "moderator", "member"
    }
}
