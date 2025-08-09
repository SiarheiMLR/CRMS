using CRMS.DAL.Data;
using CRMS.Domain.Entities;

namespace CRMS.DAL.Repositories
{
    public class EfFaqVotesRepository : Repository<FaqVote>
    {
        public EfFaqVotesRepository(CRMSDbContext context) : base(context) { }
    }
}
