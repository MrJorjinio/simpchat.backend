using Simpchat.Application.Common.Pagination;
using Simpchat.Application.Models.ApiResult;
using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IChannelService
    {
        Task<Result> AddSubscriberAsync(Guid channelId, Guid userId, Guid requesterId);
        Task<Result> JoinChannelAsync(Guid channelId, Guid userId);
        Task<Result<Guid>> CreateAsync(PostChatDto chatPostDto, UploadFileRequest? avatar);
        Task<Result> DeleteAsync(Guid channelId, Guid userId);
        Task<Result> DeleteSubscriberAsync(Guid userId, Guid channelId, Guid requesterId);
        Task<Result<List<SearchChatResponseDto>?>> SearchAsync(string searchTerm);
        Task<Result<PaginationResult<SearchChatResponseDto>>> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
        Task<Result> UpdateAsync(Guid channelId, UpdateChatDto updateChatDto, UploadFileRequest? avatar, Guid userId);
        Task<Result<List<UserChatResponseDto>>> GetUserSubscribedAsync(Guid userId);
    }
}
