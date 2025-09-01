using CRMS.DAL.Repositories;
using CRMS.Domain.DTOs;
using CRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CRMS.Domain.Interfaces;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRMS.Business.Services.FaqService
{
    public class FaqService : IFaqService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FaqService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<FaqCategory> CreateCategoryAsync(string name)
        {
            var category = new FaqCategory { Title = name };
            await _unitOfWork.FaqCategoriesRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        public async Task<List<FaqCategory>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.FaqCategoriesRepository.GetAllWithItemsAsync();
        }

        public async Task<List<FaqItem>> GetFaqsByCategoryAsync(int categoryId)
        {
            return await _unitOfWork.FaqItemsRepository.GetByCategoryAsync(categoryId) is IEnumerable<FaqItem> result
                ? result.ToList()
                : new List<FaqItem>();
        }

        public async Task<List<FaqItem>> SearchFaqsAsync(string keyword)
        {
            return await _unitOfWork.FaqItemsRepository.SearchAsync(keyword) is IEnumerable<FaqItem> result
                ? result.ToList()
                : new List<FaqItem>();
        }

        public async Task<List<FaqItem>> GetTopPopularAsync(int count)
        {
            return await _unitOfWork.FaqItemsRepository.GetTopPopularAsync(count) is IEnumerable<FaqItem> result
                ? result.ToList()
                : new List<FaqItem>();
        }

        public async Task<FaqItem?> GetByIdAsync(int id)
        {
            return await _unitOfWork.FaqItemsRepository.GetByIdAsync(id);
        }

        public async Task AddFaqAsync(FaqItem faq)
        {
            await _unitOfWork.FaqItemsRepository.AddAsync(faq);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateFaqAsync(FaqItem faq)
        {
            _unitOfWork.FaqItemsRepository.Update(faq);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteFaqAsync(int id)
        {
            var entity = await _unitOfWork.FaqItemsRepository.GetByIdAsync(id);
            if (entity is not null)
            {
                _unitOfWork.FaqItemsRepository.Remove(entity);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task AddViewAsync(int faqId)
        {
            var entity = await _unitOfWork.FaqItemsRepository.GetByIdAsync(faqId);
            if (entity is not null)
            {
                entity.Views++;
                _unitOfWork.FaqItemsRepository.Update(entity);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task VoteAsync(int faqId, int userId, bool isPositive)
        {
            if (await HasUserVotedAsync(faqId, userId)) return;

            var vote = new FaqVote
            {
                FaqItemId = faqId,
                UserId = userId,
                IsPositive = isPositive,
                VotedAt = DateTime.UtcNow
            };

            await _unitOfWork.FaqVotesRepository.AddAsync(vote);

            var faq = await _unitOfWork.FaqItemsRepository.GetByIdAsync(faqId);
            if (faq is not null)
            {
                if (isPositive)
                    faq.PositiveVotes++;
                else
                    faq.NegativeVotes++;

                _unitOfWork.FaqItemsRepository.Update(faq);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> HasUserVotedAsync(int faqId, int userId)
        {
            return await _unitOfWork.FaqItemsRepository.HasUserVotedAsync(faqId, userId);
        }

        public async Task<IEnumerable<FaqItem>> GetPopularFaqsAsync()
        {
            return (await _unitOfWork.FaqItemsRepository.GetAllAsync())
                .OrderByDescending(f => f.PositiveVotes - f.NegativeVotes)
                .Take(10);
        }

        public async Task<IEnumerable<PopularAuthorDto>> GetTopAuthorsAsync()
        {
            var allFaqs = await _unitOfWork.FaqItemsRepository.GetAllAsync();

            return allFaqs
                .Where(f => f.Author != null)
                .GroupBy(f => f.Author.DisplayName)
                .Select(g => new PopularAuthorDto
                {
                    AuthorName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        public async Task<IEnumerable<CategoryStatDto>> GetCategoryStatsAsync()
        {
            var allCategories = await _unitOfWork.FaqCategoriesRepository.GetAllAsync();
            var allFaqs = await _unitOfWork.FaqItemsRepository.GetAllAsync();

            var stats = allCategories
                .GroupJoin(allFaqs,
                           cat => cat.Id,
                           faq => faq.CategoryId,
                           (cat, faqs) => new CategoryStatDto
                           {
                               CategoryName = cat.Title,
                               FaqCount = faqs.Count()
                           })
                .ToList();

            return stats;
        }

        public async Task CreateFaqAsync(string question, string answerMarkdown, int categoryId, List<string> tags, int authorId)
        {
            var item = new FaqItem
            {
                Question = question,
                AnswerMarkdown = answerMarkdown,
                CategoryId = categoryId,
                AuthorId = authorId, // ✅ обязателен
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                PositiveVotes = 0,
                NegativeVotes = 0,
                IsPublished = true,
                Status = 0,
                Views = 0
            };

            await _unitOfWork.FaqItemsRepository.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();
            // (добавление тегов можно позже дополнить)
        }
    }
}
