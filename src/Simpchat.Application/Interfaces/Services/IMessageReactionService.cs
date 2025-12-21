using Simpchat.Application.Models.Reactions;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IMessageReactionService
    {
        /// <summary>
        /// Toggle a reaction on a message. If the reaction exists, remove it. If not, add it.
        /// </summary>
        /// <param name="messageId">The message to react to</param>
        /// <param name="reactionType">The type of reaction</param>
        /// <param name="userId">The user adding/removing the reaction</param>
        /// <returns>Result indicating if reaction was added (true) or removed (false)</returns>
        Task<Result<bool>> ToggleReactionAsync(Guid messageId, ReactionType reactionType, Guid userId);

        /// <summary>
        /// Get all reactions for a message grouped by type
        /// </summary>
        Task<Result<List<MessageReactionSummaryDto>>> GetMessageReactionsAsync(Guid messageId);
    }
}