using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Managers
{
    public class QueueManager : BaseManager
    {
        public QueueManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<Queue>> GetAllQueuesAsync() => await queueRepository.GetAllAsync();
        public async Task<Queue> GetQueueByIdAsync(int id) => await queueRepository.GetByIdAsync(id);
        public async Task AddQueueAsync(Queue queue)
        {
            await queueRepository.AddAsync(queue);
            await SaveChangesAsync();
        }
        public void UpdateQueue(Queue queue)
        {
            queueRepository.Update(queue);
            unitOfWork.SaveChanges();
        }
        public void DeleteQueue(Queue queue)
        {
            queueRepository.Remove(queue);
            unitOfWork.SaveChanges();
        }
        public async Task<IEnumerable<Queue>> FindQueuesAsync(Expression<Func<Queue, bool>> predicate)
            => await queueRepository.FindAsync(predicate);
    }
}
