using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.DAL.Repositories
{
    public class EfTransactionsRepository : Repository<Transaction>, IRepository<Transaction>
    {
        public EfTransactionsRepository(CRMSDbContext context) : base(context) { }
    }
}
