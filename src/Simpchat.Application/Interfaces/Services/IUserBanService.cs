using Simpchat.Shared.Models;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IUserBanService
    {
        /// <summary>
        /// Block a user. The blocked user will not be able to send messages or start conversations.
        /// </summary>
        Task<Result<Guid>> BlockUserAsync(Guid blockedUserId, Guid requesterId);

        /// <summary>
        /// Unblock a user.
        /// </summary>
        Task<Result> UnblockUserAsync(Guid blockedUserId, Guid requesterId);

        /// <summary>
        /// Get all users that the requester has blocked.
        /// </summary>
        Task<Result<List<BlockedUserDto>>> GetBlockedUsersAsync(Guid requesterId);

        /// <summary>
        /// Check if a user has blocked another user.
        /// </summary>
        Task<bool> IsUserBlockedAsync(Guid blockerId, Guid blockedUserId);

        /// <summary>
        /// Check if either user has blocked the other (bidirectional check).
        /// Used to prevent conversations and messages.
        /// </summary>
        Task<bool> IsEitherUserBlockedAsync(Guid userId1, Guid userId2);

        /// <summary>
        /// Check if a user can message another user (not blocked in either direction).
        /// Returns a Result with the appropriate error if blocked.
        /// </summary>
        Task<Result> CanUserMessageAsync(Guid senderId, Guid receiverId);
    }

    public class BlockedUserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime BlockedAt { get; set; }
    }
}
