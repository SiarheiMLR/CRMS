using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.DAL.Repositories
{
    public class EfCustomFieldValuesRepository : Repository<CustomFieldValue>, IRepository<CustomFieldValue>
    {
        public EfCustomFieldValuesRepository(CRMSDbContext context) : base(context) { }
    }
}
