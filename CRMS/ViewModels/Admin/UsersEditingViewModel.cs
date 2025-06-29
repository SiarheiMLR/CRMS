using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Views.Admin;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace CRMS.ViewModels.Admin
{
    public partial class UsersEditingViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private User _selectedUser;

        [ObservableProperty]
        private ObservableCollection<User> _users;

        public UsersEditingViewModel(IUserService userService, IServiceProvider serviceProvider)
        {
            _userService = userService;
            _serviceProvider = serviceProvider;
            Users = new ObservableCollection<User>();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            Users = new ObservableCollection<User>(users);
        }

        [RelayCommand]
        private void AddUser()
        {
            var editWindow = _serviceProvider.GetRequiredService<UserEditWindow>();
            var viewModel = (UserEditWindowViewModel)editWindow.DataContext;
            viewModel.Initialize(null);

            if (editWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        [RelayCommand]
        private void EditUser()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = _serviceProvider.GetRequiredService<UserEditWindow>();
            var viewModel = (UserEditWindowViewModel)editWindow.DataContext;
            viewModel.Initialize(SelectedUser);

            if (editWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        [RelayCommand]
        private async void DeleteUser()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить {SelectedUser.FirstName} {SelectedUser.LastName}?",
                "Подтвердить",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _userService.DeleteUser(SelectedUser);
                LoadUsers();
            }
        }

        // Команда для обновления списка
        [RelayCommand]
        private void Refresh()
        {
            LoadUsers();
        }
    }
}