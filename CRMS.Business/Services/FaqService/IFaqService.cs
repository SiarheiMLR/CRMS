using CRMS.Domain.Entities;
using CRMS.Domain.DTOs;

namespace CRMS.Business.Services.FaqService
{
    public interface IFaqService
    {
        Task<List<FaqCategory>> GetAllCategoriesAsync();
        Task<List<FaqItem>> GetFaqsByCategoryAsync(int categoryId);
        Task<List<FaqItem>> SearchFaqsAsync(string keyword);
        Task<List<FaqItem>> GetTopPopularAsync(int count);
        Task<FaqItem?> GetByIdAsync(int id);

        Task AddFaqAsync(FaqItem faq);
        Task UpdateFaqAsync(FaqItem faq);
        Task DeleteFaqAsync(int id);

        Task AddViewAsync(int faqId);
        Task VoteAsync(int faqId, int userId, bool isPositive);
        Task<bool> HasUserVotedAsync(int faqId, int userId);
        Task<IEnumerable<FaqItem>> GetPopularFaqsAsync();
        Task<IEnumerable<PopularAuthorDto>> GetTopAuthorsAsync();
        Task<IEnumerable<CategoryStatDto>> GetCategoryStatsAsync();
        /// <summary>
        /// Создаёт новый FAQ с автором.
        /// </summary>
        Task CreateFaqAsync(string question, string answerMarkdown, int categoryId, List<string> tags, int authorId);
        Task<FaqCategory> CreateCategoryAsync(string name);
    }
}

