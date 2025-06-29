using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.DAL.Repositories
{
    public class EfTicketsRepository : Repository<Ticket>, IRepository<Ticket>
    {
        public EfTicketsRepository(CRMSDbContext context) : base(context) { }


    }
}
