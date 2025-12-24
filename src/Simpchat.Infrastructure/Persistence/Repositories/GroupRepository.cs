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
                Id = Guid.NewGuid(),  // FIX: Explicitly generate Id to avoid duplicate key errors
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
            // Find the existing entity to avoid tracking conflicts
            var existingMember = await _dbContext.GroupsMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupMember.GroupId && m.UserId == groupMember.UserId);

            if (existingMember != null)
            {
                _dbContext.GroupsMembers.Remove(existingMember);
                await _dbContext.SaveChangesAsync();
            }
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
            // Get group IDs the user is a member of
            var groupIds = await _dbContext.GroupsMembers
                .Where(gm => gm.UserId == userId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            // Fetch groups with Members included
            return await _dbContext.Groups
                .Include(g => g.Members)
                .Where(g => groupIds.Contains(g.Id))
                .ToListAsync();
        }

        public async Task<List<Group>?> SearchAsync(string term)
        {
            return await _dbContext.Groups
                .Include(g => g.Chat)  // Include Chat entity to avoid N+1 queries
                .Where(g => EF.Functions.Like(g.Name, $"%{term}%"))
                .ToListAsync();
        }

        public async Task<(List<Group> Items, int TotalCount)> SearchPaginatedAsync(string term, int page, int pageSize)
        {
            // Require at least 3 characters for meaningful search
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return (new List<Group>(), 0);
            }

            var query = _dbContext.Groups
                .Include(g => g.Chat)
                .Where(g => g.Chat != null && g.Chat.PrivacyType == Domain.Enums.ChatPrivacyTypes.Public) // Only search public groups
                .Where(g => EF.Functions.ILike(g.Name, $"%{term}%"))
                .OrderBy(g => g.Name);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task UpdateAsync(Group entity)
        {
            _dbContext.Groups.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
