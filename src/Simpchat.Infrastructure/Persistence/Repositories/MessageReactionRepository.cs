using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    public class MessageReactionRepository : IMessageReactionRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public MessageReactionRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(MessageReaction entity)
        {
            await _dbContext.MessagesReactions.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(MessageReaction entity)
        {
            _dbContext.MessagesReactions.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<MessageReaction?> FindReactionAsync(Guid messageId, Guid userId, ReactionType reactionType)
        {
            return await _dbContext.MessagesReactions
                .FirstOrDefaultAsync(mr =>
                    mr.MessageId == messageId &&
                    mr.UserId == userId &&
                    mr.ReactionType == reactionType);
        }

        public async Task<List<MessageReaction>> GetMessageReactionsWithUsersAsync(Guid messageId)
        {
            return await _dbContext.MessagesReactions
                .Include(mr => mr.User)
                .Where(mr => mr.MessageId == messageId)
                .ToListAsync();
        }
    }
}
