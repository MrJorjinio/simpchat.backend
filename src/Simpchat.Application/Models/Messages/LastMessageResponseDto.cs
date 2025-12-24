using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Messages
{
    public class LastMessageResponseDto
    {
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public Guid? SenderId { get; set; }
        public string? SenderUsername { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public Guid? ReplyId { get; set; }
        public bool IsSeen { get; set; }
    }
}
