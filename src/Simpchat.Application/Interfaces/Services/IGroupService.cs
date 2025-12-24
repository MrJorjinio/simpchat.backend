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
    public interface IGroupService
    {
        Task<Result> AddMemberAsync(Guid groupId, Guid userId, Guid requesterId);
        Task<Result> JoinGroupAsync(Guid groupId, Guid userId);
        Task<Result<Guid>> CreateAsync(PostChatDto chatPostDto, UploadFileRequest? avatar);
        Task<Result> DeleteAsync(Guid groupId, Guid userId);
        Task<Result> DeleteMemberAsync(Guid userId, Guid groupId, Guid requesterId);
        Task<Result<List<SearchChatResponseDto>?>> SearchAsync(string searchTerm);
        Task<Result<PaginationResult<SearchChatResponseDto>>> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
        Task<Result> UpdateAsync(Guid groupId, UpdateChatDto updateChatDto, UploadFileRequest? avatar, Guid userId);
        Task<Result<List<UserChatResponseDto>>> GetUserParticipatedAsync(Guid userId);
    }
}
