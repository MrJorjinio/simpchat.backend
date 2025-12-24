using Simpchat.Application.Common.Repository;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Repositories
{
    public interface IChatBanRepository : IBaseRepository<ChatBan>
    {
        Task<Guid?> GetIdAsync(Guid chatId, Guid userId);
        Task<bool> IsUserBannedAsync(Guid chatId, Guid userId);
        Task<List<ChatBan>> GetBannedUsersAsync(Guid chatId);
    }
}
