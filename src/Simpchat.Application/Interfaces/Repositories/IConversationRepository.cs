using Simpchat.Application.Common.Repository;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Repositories
{
    public interface IConversationRepository : IBaseRepository<Conversation>
    {
        Task<Guid?> GetConversationBetweenAsync(Guid userId1, Guid userId2);
        Task<Conversation?> GetByParticipantsAsync(Guid userId1, Guid userId2);
        Task<List<Conversation>> GetUserConversationsAsync(Guid userId);
    }
}
