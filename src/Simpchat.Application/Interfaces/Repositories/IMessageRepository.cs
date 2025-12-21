using Simpchat.Application.Common.Repository;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Repositories
{
    public interface IMessageRepository : IBaseRepository<Message>
    {
        Task<Message?> GetLastMessageAsync(Guid chatId);
        Task<Message?> GetUserLastSendedMessageAsync(Guid userId, Guid chatId);
        Task<List<Message>> GetPinnedMessagesAsync(Guid chatId);
        Task<int> GetPinnedMessagesCountAsync(Guid chatId);
        Task<List<Message>> GetUnseenMessagesInChatAsync(Guid chatId, Guid userId);
        Task MarkMessagesAsSeenAsync(Guid chatId, Guid userId);
    }
}
