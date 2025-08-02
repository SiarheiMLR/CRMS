using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CRMS.ViewModels.Admin
{
    public partial class UserEditWindowViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private User _currentUser;

        [ObservableProperty]
        private string _userPassword;

        [ObservableProperty]
        private Array _roles;

        public UserEditWindowViewModel(User user = null)
        {
            _userService = App.ServiceProvider.GetService<IUserService>();
            Roles = Enum.GetValues(typeof(UserRole));
            CurrentUser = user ?? new User();
        }

        public void Initialize(User user)
        {
            if (user != null)
            {
                CurrentUser = user;
            }
            else
            {
                CurrentUser = new User();
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                CurrentUser.SetPassword(UserPassword);
                if (!(await ValidateUser())) return;

                if (CurrentUser.Id == 0) // Новый пользователь
                {
                    if (string.IsNullOrWhiteSpace(UserPassword))
                    {
                        ShowError("Пароль обязателен для нового пользователя");
                        return;
                    }
                    await _userService.AddUserAsync(CurrentUser);
                }
                else // Редактирование
                {
                    _userService.DeleteUserAsync(CurrentUser);
                }

                CloseWindow();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private async Task<bool> ValidateUser()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.UserLogonName))
            {
                ShowError("Требуется логин пользователя");
                return false;
            }
            if (string.IsNullOrWhiteSpace(CurrentUser.FirstName))
            {
                ShowError("Требуется имя пользователя");
                return false;
            }
            if (string.IsNullOrWhiteSpace(CurrentUser.LastName))
            {
                ShowError("Требуется фамилия пользователя");
                return false;
            }
            if (string.IsNullOrWhiteSpace(CurrentUser.Email))
            {
                ShowError("Требуется Email пользователя");
                return false;
            }
            if (string.IsNullOrWhiteSpace(CurrentUser.PasswordHash))
            {
                ShowError("Требуется пароль пользователя");
                return false;
            }
            if((await _userService.GetUsersByNameAsync(CurrentUser.FirstName, CurrentUser.LastName))
                .Where(u => u.Id != CurrentUser.Id)
                .Any())
            {
                ShowError("Пользователь с таким именем уже существует");
                return false;
            }
            if ((await _userService.GetUsersByEmailAsync(CurrentUser.Email))
                .Where(u => u.Id != CurrentUser.Id)
                .Any())
            {
                ShowError("Пользователь с таким Email уже существует");
                return false;
            }
            if ((await _userService.GetUsersByLoginAsync(CurrentUser.UserLogonName))
                .Where(u => u.Id != CurrentUser.Id)
                .Any())
            {
                ShowError("Пользователь с таким логином уже существует");
                return false;
            }
            return true;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = true;
                    window.Close();
                    break;
                }
            }
        }
    }
}
