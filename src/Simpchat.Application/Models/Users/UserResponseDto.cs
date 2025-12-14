using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Users
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }
        public string AvatarUrl { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public bool IsOnline { get; set; }

        public static UserResponseDto ConvertFromDomainObject(User user, IPresenceService presenceService)
        {
            if (user is null) return null;

            var response = new UserResponseDto
            {
                Id = user.Id,
                Description = user.Description,
                IsOnline = presenceService.IsUserOnline(user.Id),
                LastSeen = user.LastSeen,
                AvatarUrl = user.AvatarUrl,
                Username = user.Username
            };

            return response;
        }
    }
}
