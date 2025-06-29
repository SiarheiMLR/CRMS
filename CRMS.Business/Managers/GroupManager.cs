using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.Business.Managers
{
    public class GroupManager : BaseManager
    {
        public GroupManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync() => await groupRepository.GetAllAsync();
        public async Task<Group> GetGroupByIdAsync(int id) => await groupRepository.GetByIdAsync(id);
        public async Task AddGroupAsync(Group group)
        {
            await groupRepository.AddAsync(group);
            await SaveChangesAsync();
        }
        public void UpdateGroup(Group group)
        {
            groupRepository.Update(group);
            unitOfWork.SaveChanges();
        }
        public void DeleteGroup(Group group)
        {
            groupRepository.Remove(group);
            unitOfWork.SaveChanges();
        }
        public async Task<IEnumerable<Group>> FindGroupsAsync(Expression<Func<Group, bool>> predicate)
            => await groupRepository.FindAsync(predicate);
    }
}
