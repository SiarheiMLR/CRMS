using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;

namespace CRMS.DAL.Repositories
{
    public class EfTicketsRepository : Repository<Ticket>, IRepository<Ticket>
    {
        public EfTicketsRepository(CRMSDbContext context) : base(context) { }


    }
}
