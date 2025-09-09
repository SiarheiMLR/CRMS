using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CRMS.DAL.Repositories
{
    public class EfTransactionsRepository : Repository<Transaction>, IRepository<Transaction>
    {
        public EfTransactionsRepository(CRMSDbContext context) : base(context) { }

        public IQueryable<Transaction> AsQueryable()
        {
            return _context.Set<Transaction>().AsQueryable();
        }
    }
}
