using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Models;
using CRMS.Business.Services.AuthService;
using CRMS.Domain.Entities;
using CRMS.Services;
using CRMS.Views;
using CRMS.Views.AD;
using CRMS.Views.Admin;
using CRMS.Views.Support;
using CRMS.Views.User.TicketsPage;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;



namespace CRMS.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private bool _isMenuOpen;

        [ObservableProperty]
        private UserRole _currentRole;
        [ObservableProperty]
        private string _userName;
        [RelayCommand]
        private void ToggleMenu() => IsMenuOpen = !IsMenuOpen;

        [RelayCommand]
        private void ManageUsers()
        {
            _navigationService.NavigateTo<UsersEditingPage>();
            ToggleMenu();
        }

        [RelayCommand]
        private void UserTicketPage()
        {
            _navigationService.NavigateTo<UserTicketsPage>();
            ToggleMenu();
        }

        [RelayCommand]
        private void SupportTicketsPage()
        {
            _navigationService.NavigateTo<SupportTicketsPage>();
            ToggleMenu();
        }

        [RelayCommand]
        private void MainPage()
        {
            switch (CurrentRole)
            {
                case UserRole.Admin:
                    _navigationService.NavigateTo<MainAdminPage>();
                    break;
                case UserRole.User:
                    _navigationService.NavigateTo<MainUserPage>();
                    break;
                case UserRole.Support:
                    _navigationService.NavigateTo<MainSupportPage>();
                    break;
            }
            ToggleMenu();
        }

        public MainWindowViewModel(IAuthService authService, IServiceProvider serviceProvider,
            INavigationService navigationService)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            CurrentRole = _authService.CurrentUser.Role;
            _userName = $"{_authService.CurrentUser.FirstName} {_authService.CurrentUser.LastName}";
        }

        public object RoleSpecificContent => CurrentRole;

        [RelayCommand]
        private void ImportUsers()
        {
            var window = _serviceProvider.GetRequiredService<ADLoginWindow>();
            window.ShowDialog();
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            var window = _serviceProvider.GetRequiredService<StartUpWindow>();
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
            window?.Show();
        }

        public void NavigateToUserListPage(List<ADUserDto> users)
        {
            var userListPage = new UserListPage(users);
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.MainFrame.Navigate(userListPage);
        }
    }
}
