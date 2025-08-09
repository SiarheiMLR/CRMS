using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMS.DAL.Repositories
{
    public class EfFaqItemsRepository : Repository<FaqItem>
    {
        public EfFaqItemsRepository(CRMSDbContext context) : base(context) { }

        public async Task<IEnumerable<FaqItem>> GetByCategoryAsync(int categoryId)
        {
            return await _entities
                .Where(f => f.CategoryId == categoryId && f.IsPublished)
                .Include(f => f.Author)
                .Include(f => f.FaqItemTags).ThenInclude(t => t.FaqTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<FaqItem>> SearchAsync(string keyword)
        {
            return await _entities
                .Where(f => f.IsPublished &&
                            (f.Question.Contains(keyword) || f.AnswerMarkdown.Contains(keyword)))
                .Include(f => f.Author)
                .Include(f => f.FaqItemTags).ThenInclude(t => t.FaqTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<FaqItem>> GetTopPopularAsync(int count)
        {
            return await _entities
                .Where(f => f.IsPublished)
                .OrderByDescending(f => f.PositiveVotes - f.NegativeVotes)
                .ThenByDescending(f => f.Views)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> HasUserVotedAsync(int faqId, int userId)
        {
            return await _context.FaqVotes
                .AnyAsync(v => v.FaqItemId == faqId && v.UserId == userId);
        }

        public async Task VoteAsync(int faqId, int userId, bool isPositive)
        {
            if (await HasUserVotedAsync(faqId, userId))
                return;

            var vote = new FaqVote
            {
                FaqItemId = faqId,
                UserId = userId,
                IsPositive = isPositive,
                VotedAt = DateTime.UtcNow
            };

            await _context.FaqVotes.AddAsync(vote);

            var item = await _context.FaqItems.FindAsync(faqId);
            if (item != null)
            {
                if (isPositive) item.PositiveVotes++;
                else item.NegativeVotes++;
            }
        }

        public async Task AddViewAsync(int faqId)
        {
            var item = await _context.FaqItems.FindAsync(faqId);
            if (item != null)
            {
                item.Views++;
            }
        }
    }
}
