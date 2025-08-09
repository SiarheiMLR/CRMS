using CRMS.DAL.Data;
using CRMS.Domain.Entities;

namespace CRMS.DAL.Repositories
{
    public class EfFaqTagsRepository : Repository<FaqTag>
    {
        public EfFaqTagsRepository(CRMSDbContext context) : base(context) { }
    }
}
