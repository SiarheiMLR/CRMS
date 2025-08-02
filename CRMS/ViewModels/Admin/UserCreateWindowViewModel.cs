using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Views.Admin.Groups;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CRMS.ViewModels.Admin
{
    public partial class UserCreateWindowViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private bool _isUserCreated = false;

        [ObservableProperty]
        private User _editableUser = new User();

        [ObservableProperty]
        private bool _isInEditMode = true;

        [ObservableProperty]
        private bool _hasChanges = false;

        public UserCreateWindowViewModel(IUserService userService)
        {
            _userService = userService;
            EditableUser.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(HasChanges))
                    HasChanges = true;
            };
            EditableUser.AccountCreated = DateTime.UtcNow;
            EditableUser.Status = UserStatus.Active;
        }

        [RelayCommand]
        private void Ok()
        {
            Application.Current.Windows[1]?.Close(); // Закрыть текущее окно
        }

        [RelayCommand]
        private void Cancel()
        {
            EditableUser = new User
            {
                AccountCreated = DateTime.UtcNow,
                Status = UserStatus.Active
            };
            HasChanges = false;
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(EditableUser.FirstName) ||
                    string.IsNullOrWhiteSpace(EditableUser.LastName) ||
                    string.IsNullOrWhiteSpace(EditableUser.UserLogonName) ||
                    string.IsNullOrWhiteSpace(EditableUser.Email))
                {
                    MessageBox.Show("Заполните все обязательные поля: Имя, Фамилия, Логин, Email.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Генерация хеша пароля
                if (!string.IsNullOrWhiteSpace(EditableUser.InitialPassword))
                {
                    EditableUser.SetPassword(EditableUser.InitialPassword);
                }
                else
                {
                    MessageBox.Show("Пароль не может быть пустым.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создание пользователя
                var createdUser = await _userService.AddUserAsyncManual(EditableUser);

                if (createdUser == null)
                {
                    MessageBox.Show("Не удалось создать пользователя",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Подтверждение создания
                var dialog = new ConfirmationDialog
                {
                    Message = "Пользователь успешно создан!"
                };
                await DialogHost.Show(dialog, "EditDeleteGroupDialogHost");

                // Обновляем EditableUser с данными из БД
                EditableUser = createdUser;
                _isUserCreated = true;
                HasChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании пользователя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Edit()
        {
            IsInEditMode = true;
            HasChanges = true;
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (!_isUserCreated) return;

            var confirmDialog = new ConfirmationDialog
            {
                Message = "Вы действительно хотите удалить этого пользователя?"
            };

            var result = await DialogHost.Show(confirmDialog, "EditDeleteGroupDialogHost");
            if (result is bool confirmed && confirmed)
            {
                try
                {
                    await _userService.DeleteUserAsync(EditableUser);
                    EditableUser = new User
                    {
                        AccountCreated = DateTime.UtcNow,
                        Status = UserStatus.Active
                    };
                    _isUserCreated = false;
                    HasChanges = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void SelectAvatar()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите фото",
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                var imageBytes = File.ReadAllBytes(dialog.FileName);
                EditableUser.Avatar = imageBytes;
                HasChanges = true;
            }
        }
    }
}