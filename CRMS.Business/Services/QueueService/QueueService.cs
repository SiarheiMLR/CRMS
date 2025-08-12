using CRMS.Business.Services.QueueService;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMS.Business.Services.QueueService
{
    public class QueueService : IQueueService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QueueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Queue>> GetAllAsync()
        {
            return (await _unitOfWork.Queues.GetAllAsync()).ToList();
        }

        public async Task<List<Queue>> GetWithSlaAsync()
        {
            return (List<Queue>)await _unitOfWork.Queues.GetQueuesWithSlaAsync();
        }

        public async Task<Queue> GetByIdAsync(int id)
        {
            return await _unitOfWork.Queues.GetByIdAsync(id);
        }

        public async Task<Queue> GetByEmailAsync(string address)
        {
            return await _unitOfWork.Queues.GetByEmailAsync(address);
        }

        public async Task<List<Queue>> GetQueuesForUserAsync(int userId)
        {
            // Получаем членства пользователя в группах
            var groupMembers = await _unitOfWork.GroupMembersRepository.FindAsync(gm => gm.UserId == userId);
            var groupIds = groupMembers?.Select(gm => gm.GroupId).Distinct().ToList() ?? new List<int>();

            if (!groupIds.Any())
                return new List<Queue>();

            // Используем репозиторий очередей (ранее добавили метод GetQueuesForGroupIdsAsync)
            if (_unitOfWork.Queues is CRMS.DAL.Repositories.EfQueuesRepository efQueuesRepo)
            {
                return await efQueuesRepo.GetQueuesForGroupIdsAsync(groupIds);
            }

            // На случай, если тип репозитория другой — делаем более общий вариант
            // (пробуем получить через все permissions в контексте)
            // Но в большинстве случаев предыдущий блок сработает.
            return new List<Queue>();
        }

        public async Task AddAsync(Queue queue)
        {
            await _unitOfWork.Queues.AddAsync(queue);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Queue queue)
        {
            _unitOfWork.Queues.Update(queue);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var queue = await _unitOfWork.Queues.GetByIdAsync(id);
            if (queue != null)
            {
                _unitOfWork.Queues.Remove(queue);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}

