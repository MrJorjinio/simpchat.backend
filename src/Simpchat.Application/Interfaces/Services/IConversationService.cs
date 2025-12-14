using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Chats;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IConversationService
    {
        Task<Result<List<UserChatResponseDto>>> GetUserConversationsAsync(Guid userId);
        Task<Result> DeleteAsync(Guid conversationId, Guid userId);
    }
}
