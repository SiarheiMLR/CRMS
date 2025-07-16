using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.GroupService;
using CRMS.Domain.Entities;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CRMS.Business.Messages;

namespace CRMS.ViewModels.Admin.Groups
{
    public partial class CreateGroupViewModel : ObservableObject
    {
        private readonly IGroupService _groupService;
        public static event Action? GroupCreated;

        [ObservableProperty] private string groupName;
        [ObservableProperty] private string groupDescription;


        [ObservableProperty] private RoleOption selectedRoleOption;

        public ObservableCollection<RoleOption> AvailableRoles { get; }

        public CreateGroupViewModel(IGroupService groupService)
        {
            _groupService = groupService;

            AvailableRoles = new ObservableCollection<RoleOption>
            {
                new RoleOption { Role = UserRole.User, DisplayName = "Пользователь" },
                new RoleOption { Role = UserRole.Support, DisplayName = "Служба поддержки" },
                new RoleOption { Role = UserRole.Admin, DisplayName = "Администратор" }
            };

            SelectedRoleOption = AvailableRoles.First(); // теперь будет работать
        }

        [RelayCommand]
        private async Task CreateGroupAsync()
        {
            if (string.IsNullOrWhiteSpace(GroupName))
            {
                MessageBox.Show("Название группы обязательно.");
                return;
            }

            var selectedRole = SelectedRoleOption?.Role ?? UserRole.User;

            var group = new Group
            {
                Name = GroupName.Trim(),
                Description = GroupDescription,
                IsSystemGroup = true
            };

            await _groupService.CreateGroupAsync(group, selectedRole);

            // 📣 Отправляем сообщение
            WeakReferenceMessenger.Default.Send(new GroupCreatedMessage(group));

            MessageBox.Show($"Группа «{group.Name}» с ролью «{selectedRole}» успешно создана.");

            GroupName = GroupDescription = string.Empty;
            SelectedRoleOption = AvailableRoles.First(); // сброс выбора на "Пользователь"
        }
    }
}
