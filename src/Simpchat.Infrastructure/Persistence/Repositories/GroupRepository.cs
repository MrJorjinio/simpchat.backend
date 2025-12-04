using Microsoft.EntityFrameworkCore;
using Simpchat.Application.Common.Repository;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure.Persistence.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly SimpchatDbContext _dbContext;

        public GroupRepository(SimpchatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddMemberAsync(Guid userId, Guid groupId)
        {
            var groupMember = new GroupMember
            {
                UserId = userId,
                GroupId = groupId
            };

            await _dbContext.GroupsMembers.AddAsync(groupMember);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid> CreateAsync(Group entity)
        {
            await _dbContext.Groups.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task DeleteAsync(Group entity)
        {
            _dbContext.Groups.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteMemberAsync(GroupMember groupMember)
        {
            _dbContext.GroupsMembers.Remove(groupMember);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Group>?> GetAllAsync()
        {
            return await _dbContext.Groups.ToListAsync();
        }

        public async Task<Group?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .Include(g => g.Owner)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<Group>> GetUserParticipatedGroupsAsync(Guid userId)
        {
            return await _dbContext.GroupsMembers
                .Include(cs => cs.Group)
                .Where(cs => cs.UserId == userId)
                .Select(cs => cs.Group)
                .ToListAsync();
        }

        public async Task<List<Group>?> SearchAsync(string term)
        {
            return await _dbContext.Groups
                .Include(g => g.Chat)  // Include Chat entity to avoid N+1 queries
                .Where(g => EF.Functions.Like(g.Name, $"%{term}%"))
                .ToListAsync();
        }

        public async Task UpdateAsync(Group entity)
        {
            _dbContext.Groups.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
