using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    internal class ChatBanRepository : IChatBanRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public ChatBanRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(ChatBan entity)
        {
            await _dbContext.ChatBans.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAsync(ChatBan entity)
        {
            _dbContext.ChatBans.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ChatBan>?> GetAllAsync()
        {
            return await _dbContext.ChatBans.ToListAsync();
        }

        public async Task<ChatBan?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ChatBans.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Guid?> GetIdAsync(Guid chatId, Guid userId)
        {
            var chatBan = await _dbContext.ChatBans
                .FirstOrDefaultAsync(cb => cb.ChatId == chatId && cb.UserId == userId);

            return chatBan?.Id;
        }

        public async Task<bool> IsUserBannedAsync(Guid chatId, Guid userId)
        {
            return await _dbContext.ChatBans
                .AnyAsync(cb => cb.ChatId == chatId && cb.UserId == userId);
        }

        public async Task<List<ChatBan>> GetBannedUsersAsync(Guid chatId)
        {
            return await _dbContext.ChatBans
                .Include(cb => cb.User)
                .Where(cb => cb.ChatId == chatId)
                .ToListAsync();
        }

        public async Task UpdateAsync(ChatBan entity)
        {
            _dbContext.ChatBans.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
