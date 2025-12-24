using Simpchat.Application.Errors;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Reactions;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    internal class MessageReactionService : IMessageReactionService
    {
        private readonly IMessageReactionRepository _repo;
        private readonly IMessageRepository _messageRepo;

        public MessageReactionService(
            IMessageReactionRepository repo,
            IMessageRepository messageRepo)
        {
            _repo = repo;
            _messageRepo = messageRepo;
        }

        public async Task<Result<bool>> ToggleReactionAsync(Guid messageId, ReactionType reactionType, Guid userId)
        {
            // Check if message exists
            var message = await _messageRepo.GetByIdAsync(messageId);
            if (message is null)
            {
                return Result.Failure<bool>(ApplicationErrors.Message.IdNotFound);
            }

            // Check if user already has this reaction on this message
            var existingReaction = await _repo.FindReactionAsync(messageId, userId, reactionType);

            if (existingReaction is not null)
            {
                // Remove the reaction (undo)
                await _repo.DeleteAsync(existingReaction);
                return Result.Success(false); // false = reaction was removed
            }

            // Add the reaction
            var newReaction = new MessageReaction
            {
                MessageId = messageId,
                UserId = userId,
                ReactionType = reactionType,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.CreateAsync(newReaction);
            return Result.Success(true); // true = reaction was added
        }

        public async Task<Result<List<MessageReactionSummaryDto>>> GetMessageReactionsAsync(Guid messageId)
        {
            var reactions = await _repo.GetMessageReactionsWithUsersAsync(messageId);

            // Group reactions by type and count them
            var summary = reactions
                .GroupBy(r => r.ReactionType)
                .Select(g => new MessageReactionSummaryDto
                {
                    ReactionType = g.Key.ToString(),
                    Count = g.Count(),
                    UserIds = g.Select(r => r.UserId.ToString()).ToList()
                })
                .ToList();

            return Result.Success(summary);
        }
    }
}
