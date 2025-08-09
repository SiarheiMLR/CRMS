using CRMS.DAL.Data;
using CRMS.Domain.Entities;

namespace CRMS.DAL.Repositories
{
    public class EfFaqItemTagsRepository : Repository<FaqItemTag>
    {
        public EfFaqItemTagsRepository(CRMSDbContext context) : base(context) { }
    }
}
