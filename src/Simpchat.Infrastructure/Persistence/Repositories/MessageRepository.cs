using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Common.Repository;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public MessageRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(Message entity)
        {
            await _dbContext.Messages.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task DeleteAsync(Message entity)
        {
            _dbContext.Messages.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Message>?> GetAllAsync()
        {
            return await _dbContext.Messages
                .ToListAsync();
        }

        public async Task<Message?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Message?> GetLastMessageAsync(Guid chatId)
        {
            return await _dbContext.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Message?> GetUserLastSendedMessageAsync(Guid userId, Guid chatId)
        {
            return await _dbContext.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatId == chatId && m.SenderId == userId)
                .OrderByDescending(m => (DateTimeOffset?)m.SentAt ?? DateTimeOffset.MinValue)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Message entity)
        {
            _dbContext.Messages.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
