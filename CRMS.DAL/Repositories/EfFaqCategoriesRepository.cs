using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMS.DAL.Repositories
{
    public class EfFaqCategoriesRepository : Repository<FaqCategory>
    {
        public EfFaqCategoriesRepository(CRMSDbContext context) : base(context) { }

        /// <summary>
        /// Загружает все категории с их FAQ-элементами.
        /// </summary>
        public async Task<List<FaqCategory>> GetAllWithItemsAsync()
        {
            return await _context.FaqCategories
                .Include(c => c.Items)
                .AsNoTracking()
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        /// <summary>
        /// Загружает одну категорию по Id вместе с FAQ-элементами.
        /// </summary>
        public async Task<FaqCategory?> GetByIdWithItemsAsync(int id)
        {
            return await _context.FaqCategories
                .Include(c => c.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
