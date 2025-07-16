using CRMS.Domain.Interfaces;
using CRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMS.Business.Model;
using CRMS.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CRMS.Business.Services.GroupService
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceScopeFactory _scopeFactory;

        public GroupService(IUnitOfWork unitOfWork, IServiceScopeFactory scopeFactory)
        {
            _unitOfWork = unitOfWork;
            _scopeFactory = scopeFactory;
        }

        public async Task CreateGroupAsync(Group group)
        {
            await CreateGroupAsync(group, UserRole.User);
        }

        public async Task CreateGroupAsync(Group group, UserRole selectedRole)
        {
            var existing = await _unitOfWork.GroupsRepository.FindAsync(g => g.Name == group.Name);

            if (existing.Any())
                throw new Exception("Группа с таким названием уже существует.");

            await _unitOfWork.GroupsRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();

            var mapping = new GroupRoleMapping
            {
                GroupId = group.Id,
                Role = selectedRole
            };

            await _unitOfWork.GroupRoleMappings.AddAsync(mapping);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            var groups = await _unitOfWork.GroupsRepository.GetAllAsync();
            return groups.ToList();
        }

        public async Task<Group?> GetGroupByNameAsync(string name)
        {
            return await _unitOfWork.GroupsRepository.FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task<List<GroupWithMembers>> GetGroupsWithMembersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var groups = await scopedUnitOfWork.GroupsRepository.GetAllAsync();
            var groupList = new List<GroupWithMembers>();

            foreach (var group in groups)
            {
                var members = await (scopedUnitOfWork.GroupMembersRepository as EfGroupMembersRepository)!
                    .GetWithUsersAsync(gm => gm.GroupId == group.Id);

                var users = members.Select(gm => gm.User).ToList();

                groupList.Add(new GroupWithMembers
                {
                    Group = group,
                    Members = users
                });
            }

            return groupList;
        }

        public async Task<UserRole> GetRoleForGroupAsync(int groupId)
        {
            var mapping = await _unitOfWork.GroupRoleMappings.FirstOrDefaultAsync(m => m.GroupId == groupId);
            return mapping?.Role ?? UserRole.User;
        }

        public async Task UpdateGroupAsync(Group group, UserRole role)
        {
            _unitOfWork.GroupsRepository.Update(group);

            var mapping = await _unitOfWork.GroupRoleMappings.FirstOrDefaultAsync(m => m.GroupId == group.Id);

            if (mapping != null)
            {
                mapping.Role = role;
                _unitOfWork.GroupRoleMappings.Update(mapping);
            }
            else
            {
                await _unitOfWork.GroupRoleMappings.AddAsync(new GroupRoleMapping
                {
                    GroupId = group.Id,
                    Role = role
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            var group = await _unitOfWork.GroupsRepository.GetByIdAsync(groupId);
            if (group != null)
            {
                _unitOfWork.GroupsRepository.Remove(group);

                var mapping = await _unitOfWork.GroupRoleMappings.FirstOrDefaultAsync(m => m.GroupId == groupId);
                if (mapping != null)
                    _unitOfWork.GroupRoleMappings.Remove(mapping);

                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
