using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;

namespace Simpchat.Application.Interfaces.Repositories
{
    public interface IMessageReactionRepository
    {
        /// <summary>
        /// Create a new reaction
        /// </summary>
        Task CreateAsync(MessageReaction entity);

        /// <summary>
        /// Delete a reaction
        /// </summary>
        Task DeleteAsync(MessageReaction entity);

        /// <summary>
        /// Find a specific reaction by message, user, and reaction type
        /// </summary>
        Task<MessageReaction?> FindReactionAsync(Guid messageId, Guid userId, ReactionType reactionType);

        /// <summary>
        /// Get all reactions for a message with user info
        /// </summary>
        Task<List<MessageReaction>> GetMessageReactionsWithUsersAsync(Guid messageId);
    }
}
