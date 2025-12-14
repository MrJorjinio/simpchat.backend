using Simpchat.Application.Models.Users;
using Simpchat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Chats
{
    public class GetByIdChatProfile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Description { get; set; }
        public ChatTypes Type { get; set; }
        public ChatPrivacyTypes? Privacy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int ParticipantsCount { get; set; }
        public int ParticipantsOnline { get; set; }
        public ICollection<ChatMemberDto> Members { get; set; }
    }
}
