using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRMS.Domain.Entities;
using System.Linq.Expressions;
using CRMS.Business.Model;

namespace CRMS.Business.Services.GroupService
{
    public interface IGroupService
    {
        Task CreateGroupAsync(Group group, UserRole role);
        Task<List<Group>> GetAllGroupsAsync();
        Task<Group?> GetGroupByNameAsync(string name);
        Task<List<GroupWithMembers>> GetGroupsWithMembersAsync();
        Task<UserRole> GetRoleForGroupAsync(int groupId);
        Task UpdateGroupAsync(Group group, UserRole role);
        Task DeleteGroupAsync(int groupId);
        Task AddUserToGroupAsync(int groupId, User user);
    }
}
