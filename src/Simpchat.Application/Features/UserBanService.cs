using Simpchat.Application.Errors;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Domain.Entities;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    public class UserBanService : IUserBanService
    {
        private readonly IUserBanRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IConversationRepository _conversationRepo;

        public UserBanService(
            IUserBanRepository repo,
            IUserRepository userRepo,
            IConversationRepository conversationRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
            _conversationRepo = conversationRepo;
        }

        public async Task<Result<Guid>> BlockUserAsync(Guid blockedUserId, Guid requesterId)
        {
            // Cannot block yourself
            if (blockedUserId == requesterId)
            {
                return Result.Failure<Guid>(ApplicationErrors.UserBan.CannotBanSelf);
            }

            // Check if user exists
            var blockedUser = await _userRepo.GetByIdAsync(blockedUserId);
            if (blockedUser is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.IdNotFound);
            }

            // Check if already blocked
            if (await _repo.IsUserBannedAsync(requesterId, blockedUserId))
            {
                return Result.Failure<Guid>(ApplicationErrors.UserBan.AlreadyBanned);
            }

            // Create the ban
            var userBan = new UserBan
            {
                BlockerId = requesterId,
                BlockedUserId = blockedUserId,
                BannedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(userBan);

            // Note: Conversation is NOT deleted when blocking
            // User can use "Delete & Block" option if they want to delete conversation too

            return userBan.Id;
        }

        public async Task<Result> UnblockUserAsync(Guid blockedUserId, Guid requesterId)
        {
            // Find the existing ban
            var ban = await _repo.GetBanAsync(requesterId, blockedUserId);
            if (ban == null)
            {
                return Result.Failure(ApplicationErrors.UserBan.NotFound);
            }

            await _repo.DeleteAsync(ban);

            return Result.Success();
        }

        public async Task<Result<List<BlockedUserDto>>> GetBlockedUsersAsync(Guid requesterId)
        {
            var blockedUsers = await _repo.GetBannedUsersAsync(requesterId);

            var result = blockedUsers.Select(b => new BlockedUserDto
            {
                UserId = b.BlockedUserId,
                Username = b.BlockedUser?.Username ?? "Unknown",
                AvatarUrl = b.BlockedUser?.AvatarUrl,
                BlockedAt = b.BannedAt
            }).ToList();

            return result;
        }

        public async Task<bool> IsUserBlockedAsync(Guid blockerId, Guid blockedUserId)
        {
            return await _repo.IsUserBannedAsync(blockerId, blockedUserId);
        }

        public async Task<bool> IsEitherUserBlockedAsync(Guid userId1, Guid userId2)
        {
            return await _repo.IsEitherUserBannedAsync(userId1, userId2);
        }

        public async Task<Result> CanUserMessageAsync(Guid senderId, Guid receiverId)
        {
            // Check if sender is blocked by receiver
            if (await _repo.IsUserBannedAsync(receiverId, senderId))
            {
                return Result.Failure(ApplicationErrors.UserBan.UserBanned);
            }

            // Check if sender has blocked receiver (they need to unblock first)
            if (await _repo.IsUserBannedAsync(senderId, receiverId))
            {
                return Result.Failure(ApplicationErrors.UserBan.CannotMessageBannedUser);
            }

            return Result.Success();
        }

        private async Task DeleteExistingConversationsAsync(Guid userId1, Guid userId2)
        {
            try
            {
                // Find any existing conversations between these users
                var conversation = await _conversationRepo.GetByParticipantsAsync(userId1, userId2);
                if (conversation != null)
                {
                    await _conversationRepo.DeleteAsync(conversation);
                }
            }
            catch
            {
                // If conversation deletion fails, we don't want to fail the ban operation
                // The ban will still prevent future messages
            }
        }
    }
}
