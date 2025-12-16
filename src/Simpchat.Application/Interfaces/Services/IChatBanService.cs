
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IChatBanService
    {
        Task<Result<Guid>> BanUserAsync(Guid chatId, Guid userId, Guid requesterId);
        Task<Result> DeleteAsync(Guid chatId, Guid userId, Guid requesterId);
        Task<Result<List<BannedUserDto>>> GetBannedUsersAsync(Guid chatId, Guid requesterId);
    }

    public class BannedUserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime BannedAt { get; set; }
    }
}
