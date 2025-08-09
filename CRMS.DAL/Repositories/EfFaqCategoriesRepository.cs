using CRMS.DAL.Data;
using CRMS.Domain.Entities;

namespace CRMS.DAL.Repositories
{
    public class EfFaqCategoriesRepository : Repository<FaqCategory>
    {
        public EfFaqCategoriesRepository(CRMSDbContext context) : base(context) { }
    }
}
