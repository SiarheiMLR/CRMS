using CRMS.DAL.Data;
using CRMS.Domain.Entities;

namespace CRMS.DAL.Repositories
{
    public class EfFaqItemHistoriesRepository : Repository<FaqItemHistory>
    {
        public EfFaqItemHistoriesRepository(CRMSDbContext context) : base(context) { }
    }
}
