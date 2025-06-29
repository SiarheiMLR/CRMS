using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.DAL.Repositories
{
    public class EfGroupsRepository : Repository<Group>, IRepository<Group>
    {
        public EfGroupsRepository(CRMSDbContext context) : base(context) { }
    }
}
