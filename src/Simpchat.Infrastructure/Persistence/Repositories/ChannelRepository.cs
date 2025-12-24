using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Common.Repository;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    public class ChannelRepository : IChannelRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public ChannelRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(Channel entity)
        {
            await _dbContext.Channels.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task DeleteAsync(Channel entity)
        {
            _dbContext.Channels.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Channel>?> GetAllAsync()
        {
            return await _dbContext.Channels.ToListAsync();
        }

        public async Task<Channel?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Channels
                .Include(c => c.Subscribers)
                    .ThenInclude(s => s.User)
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Channel>> GetUserSubscribedChannelsAsync(Guid userId)
        {
            // Get channel IDs the user is subscribed to
            var channelIds = await _dbContext.ChannelsSubscribers
                .Where(cs => cs.UserId == userId)
                .Select(cs => cs.ChannelId)
                .ToListAsync();

            // Fetch channels with Subscribers included
            return await _dbContext.Channels
                .Include(c => c.Subscribers)
                .Where(c => channelIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<List<Channel>?> SearchAsync(string term)
        {
            return await _dbContext.Channels
                .Include(c => c.Chat)  // Include Chat entity to avoid N+1 queries
                .Where(c => EF.Functions.Like(c.Name, $"%{term}%"))
                .ToListAsync();
        }

        public async Task<(List<Channel> Items, int TotalCount)> SearchPaginatedAsync(string term, int page, int pageSize)
        {
            // Require at least 3 characters for meaningful search
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return (new List<Channel>(), 0);
            }

            var query = _dbContext.Channels
                .Include(c => c.Chat)
                .Where(c => c.Chat != null && c.Chat.PrivacyType == Domain.Enums.ChatPrivacyTypes.Public) // Only search public channels
                .Where(c => EF.Functions.ILike(c.Name, $"%{term}%"))
                .OrderBy(c => c.Name);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task UpdateAsync(Channel entity)
        {
            _dbContext.Channels.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
