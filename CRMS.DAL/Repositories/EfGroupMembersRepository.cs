using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMS.DAL.Repositories
{
    public class EfGroupMembersRepository : Repository<GroupMember>, IGroupMemberRepository
    {
        public EfGroupMembersRepository(CRMSDbContext context) : base(context) { }

        public async Task<IEnumerable<GroupMember>> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Where(gm => gm.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId)
        {
            return await _dbSet.Where(gm => gm.GroupId == groupId).ToListAsync();
        }
    }
}
