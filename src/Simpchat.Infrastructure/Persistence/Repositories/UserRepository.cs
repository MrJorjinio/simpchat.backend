using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Common.Repository;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public UserRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(User entity)
        {
            await _dbContext.Users.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task DeleteAsync(User entity)
        {
            _dbContext.Users.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<User>?> GetAllAsync()
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> SearchAsync(string term)
        {
            return await _dbContext.Users
                .Where(u => EF.Functions.Like(u.Username, $"%{term}%"))
                .ToListAsync();
        }

        public async Task<(List<User> Items, int TotalCount)> SearchPaginatedAsync(string term, int page, int pageSize)
        {
            // Require at least 3 characters for meaningful search
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return (new List<User>(), 0);
            }

            var query = _dbContext.Users
                .Where(u => EF.Functions.ILike(u.Username, $"%{term}%"))
                .OrderBy(u => u.Username);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task UpdateAsync(User entity)
        {
            _dbContext.Users.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
