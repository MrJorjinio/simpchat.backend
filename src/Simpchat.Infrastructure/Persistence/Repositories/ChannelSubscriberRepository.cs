using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    public class ChannelSubscriberRepository : IChannelSubscriberRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public ChannelSubscriberRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(ChannelSubscriber entity)
        {
            await _dbContext.ChannelsSubscribers.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAsync(ChannelSubscriber entity)
        {
            _dbContext.ChannelsSubscribers.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ChannelSubscriber>?> GetAllAsync()
        {
            return await _dbContext.ChannelsSubscribers.ToListAsync();
        }

        public async Task<ChannelSubscriber?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ChannelsSubscribers
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(ChannelSubscriber entity)
        {
            _dbContext.ChannelsSubscribers.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteSubscriberAsync(Guid channelId, Guid userId)
        {
            var existingSubscriber = await _dbContext.ChannelsSubscribers
                .FirstOrDefaultAsync(s => s.ChannelId == channelId && s.UserId == userId);

            if (existingSubscriber != null)
            {
                _dbContext.ChannelsSubscribers.Remove(existingSubscriber);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
