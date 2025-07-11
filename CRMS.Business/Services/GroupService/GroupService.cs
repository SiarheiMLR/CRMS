using CRMS.Domain.Interfaces;
using CRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMS.Business.Model;
using CRMS.DAL.Repositories;

namespace CRMS.Business.Services.GroupService
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateGroupAsync(Group group)
        {
            // По умолчанию присваиваем роль User, если в интерфейсе ожидается этот метод
            await CreateGroupAsync(group, UserRole.User);
        }

        public async Task CreateGroupAsync(Group group, UserRole selectedRole)
        {
            var existing = await _unitOfWork.GroupsRepository
                .FindAsync(g => g.Name == group.Name);

            if (existing.Any())
                throw new Exception("Группа с таким названием уже существует.");

            await _unitOfWork.GroupsRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync(); // сохранили группу и получили её Id

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
            var groups = await _unitOfWork.GroupsRepository.GetAllAsync();
            var groupList = new List<GroupWithMembers>();

            foreach (var group in groups)
            {
                var members = await (_unitOfWork.GroupMembersRepository as EfGroupMembersRepository)!
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

    }
}
