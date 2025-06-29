using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.DAL.Repositories
{
    public class EfCustomFieldsRepository : Repository<CustomField>, IRepository<CustomField>
    {
        public EfCustomFieldsRepository(CRMSDbContext context) : base(context) { }
    }
}
