using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Model;
using CRMS.Business.Services.GroupService;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using CRMS.Views.Admin.Groups;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using CRMS.Business.Messages;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


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

        public GroupOverviewViewModel(
            IGroupService groupService,
            IUnitOfWork unitOfWork,
            IServiceProvider serviceProvider)
        {
            _groupService = groupService;
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
            RegisterMessenger();
            _ = LoadGroupsAsync(); // фоновая загрузка
        }

        [RelayCommand]
        private async Task LoadGroupsAsync()
        {
            var groupEntities = await _groupService.GetGroupsWithMembersAsync();
            Groups = new ObservableCollection<GroupWithMembers>(groupEntities);
        }

        [RelayCommand]
        private async Task AddUserToGroupAsync(GroupWithMembers groupWithMembers)
        {
            var window = _serviceProvider.GetRequiredService<AddUserToGroupWindow>();
            if (window.DataContext is AddUserToGroupViewModel vm)
                await vm.SetGroupAsync(groupWithMembers.Group);

            window.ShowDialog();
            await LoadGroupsAsync();
        }

        [RelayCommand]
        private async Task RemoveUserFromGroup(GroupWithMembers groupWithMembers)
        {
            if (SelectedMember == null)
            {
                MessageBox.Show("Не выбран пользователь для удаления.");
                return;
            }

            var userId = SelectedMember.Id;
            var groupId = groupWithMembers.Group.Id;
            var userName = SelectedMember.DisplayName;
            var groupName = groupWithMembers.Name;

            // Показываем диалог подтверждения
            var dialog = new ConfirmationDialog
            {
                Title = "Подтверждение удаления",
                Message = $"Вы действительно хотите удалить пользователя {userName} из группы {groupName}?",
                YesButtonText = "Да, удалить",
                NoButtonText = "Отмена"
            };

            var result = await DialogHost.Show(dialog, "EditDeleteGroupDialogHost");

            // Отладка: покажем, что вернул диалог
            if (result is not bool confirmed || !confirmed)
            {
                MessageBox.Show("Удаление отменено пользователем.");
                return;
            }

            try
            {
                // 1. Найти объект связи GroupMember
                var member = await _unitOfWork.GroupMembersRepository
                    .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

                if (member == null)
                {
                    MessageBox.Show("Связь между пользователем и группой не найдена.");
                    return;
                }

                // 2. Удалить из репозитория
                _unitOfWork.GroupMembersRepository.Remove(member);

                // 3. Сохранить изменения
                int removed = await _unitOfWork.SaveChangesAsync();
                if (removed == 0)
                {
                    MessageBox.Show("Удаление не было сохранено в базе данных.");
                    return;
                }

                // 4. Перерасчитать роль пользователя
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден после удаления.");
                    return;
                }

                user.Role = RoleMapper.ResolveRole(user);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                // 5. Обновить UI
                await LoadGroupsAsync();
                SelectedMember = null;

                MessageBox.Show($"Пользователь {userName} успешно удалён из группы {groupName}.",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }                        
        }

        private void RegisterMessenger()
        {
            WeakReferenceMessenger.Default.Register<GroupCreatedMessage>(this, async (_, _) =>
            {
                await LoadGroupsAsync();
            });

            WeakReferenceMessenger.Default.Register<GroupUpdatedMessage>(this, async (_, _) =>
            {
                await LoadGroupsAsync();
            });

            WeakReferenceMessenger.Default.Register<GroupDeletedMessage>(this, async (_, _) =>
            {
                await Task.Delay(100);
                await LoadGroupsAsync();
            });
        }
    }
}