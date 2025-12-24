using Simpchat.Application.Common.Repository;
using Simpchat.Domain.Entities;

namespace Simpchat.Application.Interfaces.Repositories
{
    public interface IUserBanRepository : IBaseRepository<UserBan>
    {
        /// <summary>
        /// Get the ban record ID if it exists
        /// </summary>
        Task<Guid?> GetIdAsync(Guid blockerId, Guid blockedUserId);

        /// <summary>
        /// Check if a user has banned another user
        /// </summary>
        Task<bool> IsUserBannedAsync(Guid blockerId, Guid blockedUserId);

        /// <summary>
        /// Check if either user has banned the other (bidirectional check)
        /// </summary>
        Task<bool> IsEitherUserBannedAsync(Guid userId1, Guid userId2);

        /// <summary>
        /// Get all users that a specific user has banned
        /// </summary>
        Task<List<UserBan>> GetBannedUsersAsync(Guid blockerId);

        /// <summary>
        /// Get all users that have banned a specific user
        /// </summary>
        Task<List<UserBan>> GetUsersThatBannedAsync(Guid blockedUserId);

        /// <summary>
        /// Get a specific ban record
        /// </summary>
        Task<UserBan?> GetBanAsync(Guid blockerId, Guid blockedUserId);
    }
}
