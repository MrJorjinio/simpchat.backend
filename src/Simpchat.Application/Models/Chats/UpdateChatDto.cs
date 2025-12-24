using Simpchat.Application.Models.Files;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Chats
{
    public class UpdateChatDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
