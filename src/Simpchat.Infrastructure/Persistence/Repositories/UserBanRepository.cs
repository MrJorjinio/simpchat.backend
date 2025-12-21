using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    internal class UserBanRepository : IUserBanRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public UserBanRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(UserBan entity)
        {
            await _dbContext.UserBans.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAsync(UserBan entity)
        {
            _dbContext.UserBans.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UserBan>?> GetAllAsync()
        {
            return await _dbContext.UserBans.ToListAsync();
        }

        public async Task<UserBan?> GetByIdAsync(Guid id)
        {
            return await _dbContext.UserBans.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Guid?> GetIdAsync(Guid blockerId, Guid blockedUserId)
        {
            var userBan = await _dbContext.UserBans
                .FirstOrDefaultAsync(ub => ub.BlockerId == blockerId && ub.BlockedUserId == blockedUserId);

            return userBan?.Id;
        }

        public async Task<bool> IsUserBannedAsync(Guid blockerId, Guid blockedUserId)
        {
            return await _dbContext.UserBans
                .AnyAsync(ub => ub.BlockerId == blockerId && ub.BlockedUserId == blockedUserId);
        }

        public async Task<bool> IsEitherUserBannedAsync(Guid userId1, Guid userId2)
        {
            return await _dbContext.UserBans
                .AnyAsync(ub =>
                    (ub.BlockerId == userId1 && ub.BlockedUserId == userId2) ||
                    (ub.BlockerId == userId2 && ub.BlockedUserId == userId1));
        }

        public async Task<List<UserBan>> GetBannedUsersAsync(Guid blockerId)
        {
            return await _dbContext.UserBans
                .Include(ub => ub.BlockedUser)
                .Where(ub => ub.BlockerId == blockerId)
                .OrderByDescending(ub => ub.BannedAt)
                .ToListAsync();
        }

        public async Task<List<UserBan>> GetUsersThatBannedAsync(Guid blockedUserId)
        {
            return await _dbContext.UserBans
                .Include(ub => ub.Blocker)
                .Where(ub => ub.BlockedUserId == blockedUserId)
                .ToListAsync();
        }

        public async Task<UserBan?> GetBanAsync(Guid blockerId, Guid blockedUserId)
        {
            return await _dbContext.UserBans
                .Include(ub => ub.BlockedUser)
                .Include(ub => ub.Blocker)
                .FirstOrDefaultAsync(ub => ub.BlockerId == blockerId && ub.BlockedUserId == blockedUserId);
        }

        public async Task UpdateAsync(UserBan entity)
        {
            _dbContext.UserBans.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
