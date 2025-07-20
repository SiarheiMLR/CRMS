using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Models;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CRMS.ViewModels.Admin
{
    public partial class UserListPageViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private ObservableCollection<ADUserDto> users;

        [ObservableProperty]
        private ADUserDto? selectedUser;

        partial void OnSelectedUserChanged(ADUserDto? oldValue, ADUserDto? newValue)
        {
            AddSelectedUserCommand.NotifyCanExecuteChanged();
        }

        public UserListPageViewModel(IUserService userService, List<ADUserDto> importedUsers)
        {
            _userService = userService;
            Users = new ObservableCollection<ADUserDto>(importedUsers);
        }

        [RelayCommand(CanExecute = nameof(CanAddUser))]
        private async void AddSelectedUser()
        {
            if (SelectedUser is null)
                return;

            string email = SelectedUser.Email?.Trim().ToLower() ?? string.Empty;
            string loginName = SelectedUser.UserLogonName?.Trim().ToLower() ?? string.Empty;

            // если есть домен в email, добавим к UserLogonName
            if (!string.IsNullOrWhiteSpace(email) && email.Contains("@") && !loginName.Contains("@"))
            {
                var domain = email.Split('@')[1];
                loginName = $"{loginName}@{domain}";
            }

            // Проверка на существующего пользователя по email
            var existingUsersByEmail = await _userService.GetUsersByEmailAsync(email);
            if (existingUsersByEmail.Any())
            {
                MessageBox.Show($"Пользователь с email '{email}' уже существует в системе.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на существующего пользователя по логину
            var existingUsersByLogin = await _userService.GetUsersByLoginAsync(loginName);
            if (existingUsersByLogin.Any())
            {
                MessageBox.Show($"Пользователь с логином '{loginName}' уже существует в системе.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём нового пользователя
            var user = new User
            {
                FirstName = SelectedUser.FirstName,
                LastName = SelectedUser.LastName,
                DisplayName = SelectedUser.DisplayName,
                Email = email,
                UserLogonName = loginName,
                Office = SelectedUser.Office,
                WorkPhone = SelectedUser.WorkPhone,
                MobilePhone = SelectedUser.MobilePhone,
                IPPhone = SelectedUser.IPPhone,
                JobTitle = SelectedUser.JobTitle,
                Department = SelectedUser.Department,
                Company = SelectedUser.Company,
                ManagerName = SelectedUser.ManagerName,
                Description = SelectedUser.Description,
                Street = SelectedUser.Street,
                City = SelectedUser.City,
                State = SelectedUser.State,
                PostalCode = SelectedUser.PostalCode,
                Country = SelectedUser.Country,
                WebPage = SelectedUser.WebPage,
                DateOfBirth = SelectedUser.DateOfBirth,
                Avatar = SelectedUser.Avatar,
                Role = UserRole.User,
                Status = UserStatus.Active
            };

            string tempPassword = $"AD!{Guid.NewGuid():N}".Substring(0, 8);
            user.SetPassword(tempPassword);
            user.InitialPassword = tempPassword;

            await _userService.AddUserAsync(user);

            MessageBox.Show($"Пользователь {user.DisplayName} добавлен.\nПароль: {tempPassword}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanAddUser() => SelectedUser != null;
    }
}
