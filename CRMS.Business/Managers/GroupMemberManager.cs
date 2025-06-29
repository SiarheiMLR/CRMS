using System.Collections.Generic;
using System.Threading.Tasks;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.Business.Managers
{
    public class GroupMemberManager : BaseManager
    {
        public GroupMemberManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<GroupMember>> GetAllGroupMembersAsync() => await groupMemberRepository.GetAllAsync();
        public async Task<GroupMember> GetGroupMemberByIdAsync(int id) => await groupMemberRepository.GetByIdAsync(id);
        public async Task<IEnumerable<GroupMember>> GetGroupMembersByUserIdAsync(int userId) =>
            await groupMemberRepository.FindAsync(gm => gm.UserId == userId);
        public async Task<IEnumerable<GroupMember>> GetGroupMembersByGroupIdAsync(int groupId) =>
            await groupMemberRepository.FindAsync(gm => gm.GroupId == groupId);
        public async Task AddGroupMemberAsync(GroupMember groupMember)
        {
            await groupMemberRepository.AddAsync(groupMember);
            await SaveChangesAsync();
        }
        public void UpdateGroupMember(GroupMember groupMember)
        {
            groupMemberRepository.Update(groupMember);
            unitOfWork.SaveChanges();
        }
        public void DeleteGroupMember(GroupMember groupMember)
        {
            groupMemberRepository.Remove(groupMember);
            unitOfWork.SaveChanges();
        }
    }
}
