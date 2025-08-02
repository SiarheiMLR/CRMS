using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Views.Admin;
using CRMS.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using CRMS.Views.AD;
using CRMS.Business.Services.AuthService;
using CRMS.ViewModels.AD;
using CRMS.Views;

namespace CRMS.ViewModels.Admin
{
    public partial class UsersOverviewPageViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private ObservableCollection<CRMS.Domain.Entities.User> users = new();

        public UsersOverviewPageViewModel(
            IUserService userService,
            IAuthService authService,
            IServiceProvider serviceProvider,
            INavigationService navigationService)
        {
            _userService = userService;
            _authService = authService;
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;

            LoadUsers();
        }

        private async void LoadUsers()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            // Обновляем роли на основе групп
            foreach (var user in allUsers)
            {
                user.Role = RoleMapper.ResolveRole(user);
            }
            Users = new ObservableCollection<CRMS.Domain.Entities.User>(allUsers);
        }

        [RelayCommand]
        private void OpenUserProfile(CRMS.Domain.Entities.User user)
        {
            var userProfileWindow = new UserProfileWindow(user, _userService); // <-- используем _userService из DI
            userProfileWindow.ShowDialog();
            LoadUsers(); // Всегда обновляем список после закрытия окна
        }

        [RelayCommand]
        private void CreateUser()
        {
            var window = new UserCreateWindow(_userService);
            window.ShowDialog();
            LoadUsers(); // Обновляем список пользователей после закрытия окна
        }

        [RelayCommand]
        private void ImportUsers()
        {
            //var window = _serviceProvider.GetRequiredService<ADLoginWindow>();
            //window.ShowDialog();
            if (ADLoginWindowViewModel.CachedUsers != null)
            {
                var users = ADLoginWindowViewModel.CachedUsers;
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWindow?.MainFrame.Navigate(new Views.Admin.UserListPage(users));
                return;
            }
            else
            {
                var window = _serviceProvider.GetRequiredService<ADLoginWindow>();
                window.ShowDialog();
            }
        }
    }
}

