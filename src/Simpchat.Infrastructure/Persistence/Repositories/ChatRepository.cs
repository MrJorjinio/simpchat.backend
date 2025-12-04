using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    internal class ChatRepository : IChatRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public ChatRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddUserPermissionAsync(ChatUserPermission chatUserPermission)
        {
            await _dbContext.ChatsUsersPermissions.AddAsync(chatUserPermission);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid> CreateAsync(Chat entity)
        {
            await _dbContext.Chats.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task DeleteAsync(Chat entity)
        {
            _dbContext.Chats.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Chat>?> GetAllAsync()
        {
            return await _dbContext.Chats.ToListAsync();
        }

        public async Task<Chat?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Chats
                .Include(c => c.Messages.OrderBy(m => m.SentAt))
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Chat entity)
        {
            _dbContext.Chats.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
