using Simpchat.Application.Common.Pagination;
using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Users;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result<List<GetAllUserDto>>> GetAllAsync();
        Task<Result<List<SearchChatResponseDto>>> SearchAsync(string term, Guid userId);
        Task<Result<PaginationResult<SearchChatResponseDto>>> SearchPaginatedAsync(string term, Guid userId, int page, int pageSize);
        Task<Result<GetByIdUserDto>> GetByIdAsync(Guid userId, Guid currentUserId);
        Task<Result> UpdateAsync(Guid userId, UpdateUserDto updateUserInfoDto, UploadFileRequest avatar);
        Task<Result> SetLastSeenAsync(Guid userId);
        Task<Result> DeleteAsync(Guid userId);
    }
}
