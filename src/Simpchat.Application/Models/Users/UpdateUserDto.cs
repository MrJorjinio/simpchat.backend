using Simpchat.Application.Models.Files;
using Simpchat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Users
{
    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string? Description { get; set; }
        public HwoCanAddYouTypes AddChatMinLvl { get; set; }
    }
}
