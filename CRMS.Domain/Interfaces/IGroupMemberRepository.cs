using CRMS.
using CRMS.Domain.Entities;

namespace CRMS.Domain.Interfaces
{
    public interface IGroupMemberRepository : IRepository<GroupMember>
    {
        Task<IEnumerable<GroupMember>> GetByUserIdAsync(int userId);
        Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId);
    }
}
