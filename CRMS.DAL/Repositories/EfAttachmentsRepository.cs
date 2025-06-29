using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.DAL.Repositories
{
    public class EfAttachmentsRepository : Repository<Attachment>, IRepository<Attachment>
    {
        public EfAttachmentsRepository(CRMSDbContext context) : base(context) { }
    }
}
