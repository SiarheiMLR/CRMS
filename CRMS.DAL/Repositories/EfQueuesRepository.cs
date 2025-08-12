using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;

namespace CRMS.DAL.Repositories
{
    public class EfQueuesRepository : Repository<Queue>, IRepository<Queue>
    {
        public EfQueuesRepository(CRMSDbContext context) : base(context) { }
    }
}
