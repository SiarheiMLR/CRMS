using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMS.DAL.Repositories
{
    public class EfUsersRepository : Repository<User>, IRepository<User>
    {
        public EfUsersRepository(CRMSDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _entities.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByNameAsync(string firstName, string lastName)
        {
            return await _entities.SingleOrDefaultAsync(u => u.FirstName == firstName && u.LastName == lastName);
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }
    }
}
