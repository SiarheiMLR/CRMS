using CRMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRMS.Business.Services.QueueService
{
    public interface IQueueService
    {
        public Task<List<Queue>> GetAllAsync();
        public Task<List<Queue>> GetWithSlaAsync();
        public Task<Queue> GetByIdAsync(int id);
        public Task<Queue> GetByEmailAsync(string address);
        public Task<List<Queue>> GetQueuesForUserAsync(int userId);
        public Task AddAsync(Queue queue);
        public Task UpdateAsync(Queue queue);
        public Task DeleteAsync(int id);
        
    }
}

