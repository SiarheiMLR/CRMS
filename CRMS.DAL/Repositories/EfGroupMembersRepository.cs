using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CRMS.DAL.Repositories
{
    public class EfGroupMembersRepository : Repository<GroupMember>, IRepository<GroupMember>
    {
        private readonly CRMSDbContext _context;

        public EfGroupMembersRepository(CRMSDbContext context) : base(context)
        {
            _context = context;
        }

        // Метод: получить список участников с подгрузкой связанных пользователей
        public async Task<List<GroupMember>> GetWithUsersAsync(Expression<Func<GroupMember, bool>> predicate)
        {
            return await _context.GroupMembers
                .Include(gm => gm.User)
                .Where(predicate)
                .ToListAsync();
        }
    }
}

