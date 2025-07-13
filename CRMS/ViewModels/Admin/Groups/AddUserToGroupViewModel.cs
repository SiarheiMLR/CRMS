using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using CRMS.Infrastructure.Converters;
using CRMS.Views.Admin.Groups;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CRMS.ViewModels.Admin.Groups
{
    public partial class AddUserToGroupViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;

        private Group _currentGroup;

        [ObservableProperty] private ObservableCollection<User> availableUsers = new();
        [ObservableProperty] private ObservableCollection<User> selectedUsers = new();

        public AddUserToGroupViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async void SetGroup(Group group)
        {
            _currentGroup = group;

            // Загружаем ВСЕХ пользователей
            var allUsers = await _unitOfWork.Users.GetAllAsync();

            // Получаем Id участников текущей группы
            var members = await _unitOfWork.GroupMembersRepository
                .GetWhereNoTrackingAsync(gm => gm.GroupId == group.Id);
            var memberIds = members.Select(m => m.UserId).ToHashSet();

            // Оставляем только тех, кто еще не в группе
            var notInGroup = allUsers.Where(u => !memberIds.Contains(u.Id)).ToList(); ;
            AvailableUsers = new ObservableCollection<User>(notInGroup);
        }

        [RelayCommand]
        private async Task AddSelectedUsersAsync()
        {
            if (_currentGroup is null || SelectedUsers.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одного пользователя.");
                return;
            }

            foreach (var user in SelectedUsers)
            {
                var member = new GroupMember
                {
                    GroupId = _currentGroup.Id,
                    UserId = user.Id
                };

                await _unitOfWork.GroupMembersRepository.AddAsync(member);
                user.Role = RoleMapper.ResolveRole(user); // обновить роль
            }

            await _unitOfWork.SaveChangesAsync();
            MessageBox.Show("Пользователи добавлены в группу.");
            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is AddUserToGroupWindow)?.Close();
        }
    }
}

