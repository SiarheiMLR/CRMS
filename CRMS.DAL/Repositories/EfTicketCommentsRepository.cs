using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace CRMS.DAL.Repositories
{
    public class EfTicketCommentsRepository : Repository<TicketComment>, IRepository<TicketComment>
    {
        public EfTicketCommentsRepository(CRMSDbContext context) : base(context) { }        
    }
}
