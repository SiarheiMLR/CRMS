using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Views.Admin;
using Microsoft.VisualBasic.ApplicationServices;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CRMS.ViewModels.Admin
{
    public partial class UsersOverviewPageViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private ObservableCollection<CRMS.Domain.Entities.User> users = new();

        public UsersOverviewPageViewModel(IUserService userService)
        {
            _userService = userService;
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
            var window = new UserProfileWindow(user); // передаём данные
            window.ShowDialog();
        }
    }
}

