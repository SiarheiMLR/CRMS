using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Model;
using CRMS.Business.Services.GroupService;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using CRMS.Views.Admin.Groups;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CRMS.ViewModels.Admin.Groups
{
    public partial class GroupOverviewViewModel : ObservableObject
    {
        private readonly IGroupService _groupService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableCollection<GroupWithMembers> groups = new();

        [ObservableProperty]
        private User? selectedMember;

        public IRelayCommand<(Group group, User user)> RemoveUserFromGroupCommand { get; }

        public GroupOverviewViewModel(
            IGroupService groupService,
            IUnitOfWork unitOfWork,
            IServiceProvider serviceProvider)
        {
            _groupService = groupService;
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;

            RemoveUserFromGroupCommand = new RelayCommand<(Group group, User user)>(async tuple =>
            {
                var (group, user) = tuple;
                await RemoveUserFromGroupAsync(group, user);
            });

            LoadGroupsAsync();
        }

        [RelayCommand]
        private async Task LoadGroupsAsync()
        {
            var groupEntities = await _groupService.GetGroupsWithMembersAsync();
            Groups = new ObservableCollection<GroupWithMembers>(groupEntities);
        }

        [RelayCommand]
        private void AddUserToGroup(GroupWithMembers groupWithMembers)
        {
            var window = _serviceProvider.GetRequiredService<AddUserToGroupWindow>();
            window.SetGroup(groupWithMembers.Group);
            window.ShowDialog();

            _ = LoadGroupsAsync();
        }

        private async Task RemoveUserFromGroupAsync(Group group, User user)
        {
            var member = await _unitOfWork.GroupMembersRepository
                .FirstOrDefaultAsync(gm => gm.GroupId == group.Id && gm.UserId == user.Id);

            if (member is not null)
            {
                _unitOfWork.GroupMembersRepository.Remove(member);
                await _unitOfWork.SaveChangesAsync();

                user.Role = RoleMapper.ResolveRole(user);
            }

            await LoadGroupsAsync();
        }
    }
}
