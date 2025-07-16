using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.GroupService;
using CRMS.Domain.Entities;
using CRMS.Views.Admin.Groups;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CRMS.Business.Messages;

namespace CRMS.ViewModels.Admin.Groups
{
    public partial class EditDeleteGroupViewModel : ObservableObject
    {
        private readonly IGroupService _groupService;

        public static event Action? GroupUpdatedOrDeleted;

        [ObservableProperty] private Group? selectedGroup;
        [ObservableProperty] private string groupName = string.Empty;
        [ObservableProperty] private string groupDescription = string.Empty;
        [ObservableProperty] private RoleOption? selectedRoleOption;

        public ObservableCollection<Group> Groups { get; }
        public ObservableCollection<RoleOption> AvailableRoles { get; }

        public EditDeleteGroupViewModel(IGroupService groupService)
        {
            _groupService = groupService;

            Groups = new ObservableCollection<Group>();
            AvailableRoles = new ObservableCollection<RoleOption>
            {
                new RoleOption { Role = UserRole.User, DisplayName = "Пользователь" },
                new RoleOption { Role = UserRole.Support, DisplayName = "Служба поддержки" },
                new RoleOption { Role = UserRole.Admin, DisplayName = "Администратор" }
            };

            // 🧠 Асинхронно загружаем группы после конструктора
            _ = LoadGroupsAsync();
        }

        private async Task LoadGroupsAsync()
        {
            var allGroups = await _groupService.GetAllGroupsAsync();
            App.Current.Dispatcher.Invoke(() =>
            {
                Groups.Clear();
                foreach (var group in allGroups)
                    Groups.Add(group);
            });
        }

        partial void OnSelectedGroupChanged(Group? value)
        {
            if (value == null)
            {
                GroupName = GroupDescription = string.Empty;
                SelectedRoleOption = null;
                return;
            }

            GroupName = value.Name;
            GroupDescription = value.Description ?? string.Empty;

            _ = LoadGroupRoleAsync(value.Id);
        }

        private async Task LoadGroupRoleAsync(int groupId)
        {
            var groupRole = await _groupService.GetRoleForGroupAsync(groupId);
            SelectedRoleOption = AvailableRoles.FirstOrDefault(r => r.Role == groupRole)
                                 ?? AvailableRoles.First();
        }

        [RelayCommand]
        private async Task UpdateGroupAsync()
        {
            if (SelectedGroup == null)
            {
                MessageBox.Show("Выберите группу для редактирования.");
                return;
            }

            if (string.IsNullOrWhiteSpace(GroupName))
            {
                MessageBox.Show("Название группы обязательно.");
                return;
            }

            SelectedGroup.Name = GroupName.Trim();
            SelectedGroup.Description = GroupDescription;
            var role = SelectedRoleOption?.Role ?? UserRole.User;

            await _groupService.UpdateGroupAsync(SelectedGroup, role);

            // 📣 Уведомление через Messenger
            WeakReferenceMessenger.Default.Send(new GroupUpdatedMessage(SelectedGroup));

            MessageBox.Show($"Группа «{SelectedGroup.Name}» успешно обновлена.");            
        }

        [RelayCommand]
        private async Task DeleteGroupAsync()
        {
            if (SelectedGroup == null)
            {
                MessageBox.Show("Выберите группу для удаления.");
                return;
            }

            var dialog = new ConfirmationDialog
            {
                Title = "Подтверждение удаления",
                Message = $"Вы действительно хотите удалить группу «{SelectedGroup.Name}»?",
                YesButtonText = "Удалить",
                NoButtonText = "Отмена"
            };

            // Открытие диалога и ожидание ответа
            var result = await DialogHost.Show(dialog, "EditDeleteGroupDialogHost");

            if (!(result is bool confirmed) || !confirmed)
                return;

            int deletedId = SelectedGroup.Id;

            await _groupService.DeleteGroupAsync(deletedId);

            // 📣 Уведомление через Messenger
            WeakReferenceMessenger.Default.Send(new GroupDeletedMessage(deletedId));

            Groups.Remove(SelectedGroup);
            SelectedGroup = null;           

            MessageBox.Show("Группа удалена.");            
        }
    }
}
