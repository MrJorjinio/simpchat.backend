using Simpchat.Application.Models.ApiResult;
using Simpchat.Application.Models.Chats;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IChatService
    {
        Task<Result> UpdatePrivacyTypeAsync(Guid chatId, ChatPrivacyTypes chatPrivacyType, Guid userId);
        Task<Result<GetByIdChatProfile>> GetProfileAsync(Guid chatId, Guid userId);
        Task<Result<GetByIdChatDto>> GetByIdAsync(Guid chatId, Guid userId);
        Task<Result<List<UserChatResponseDto>>> GetUserChatsAsync(Guid userId);
        Task<Result<List<SearchChatResponseDto>>> SearchAsync(string term, Guid userId);
        Task<Result<Guid>> AddUserPermissionAsync(Guid chatId, Guid userId, string permissionName, Guid requesterId);
        /// <summary>
        /// Lightweight method to get basic chat info (name, avatar, member IDs) without heavy queries
        /// </summary>
        Task<Result<ChatBasicInfoDto>> GetBasicInfoAsync(Guid chatId);
    }

    /// <summary>
    /// Lightweight DTO for basic chat info (used for notifications)
    /// </summary>
    public class ChatBasicInfoDto
    {
        public string Name { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public List<Guid> MemberIds { get; set; } = new();
    }
}
