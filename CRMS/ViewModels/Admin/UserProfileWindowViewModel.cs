using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CRMS.ViewModels.Admin
{
    public partial class UserProfileWindowViewModel : ObservableObject
    {
        // Поля
        private readonly IUserService _userService;
        private CRMS.Domain.Entities.User _originalUser;

        // Observable свойства (сначала поля, затем флаги)
        [ObservableProperty]
        private CRMS.Domain.Entities.User editableUser;

        [ObservableProperty]
        private bool isInEditMode = false;

        [ObservableProperty]
        private bool hasChanges = false;

        // Команды
        public IRelayCommand EditCommand { get; }

        public IAsyncRelayCommand SaveAsyncCommand { get; }

        public IAsyncRelayCommand DeleteAsyncCommand { get; }

        // Конструктор
        public UserProfileWindowViewModel(CRMS.Domain.Entities.User user, IUserService userService)
        {
            _userService = userService;

            _originalUser = CloneUser(user);
            EditableUser = CloneUser(user); // рабочая копия
            EditableUser.PropertyChanged += EditableUser_PropertyChanged;

            EditCommand = new RelayCommand(StartEditing);
            SaveAsyncCommand = new AsyncRelayCommand(SaveAsync);
            DeleteAsyncCommand = new AsyncRelayCommand(DeleteAsync);
        }

        // Обработчики событий
        private void EditableUser_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsInEditMode)
            {
                HasChanges = true;
            }
        }
        // Команды
        #region Команды

        [RelayCommand]
        private void StartEditing()
        {
            if (_originalUser == null)
                return;

            // Отписываемся от старого (на всякий случай)
            if (EditableUser != null)
                EditableUser.PropertyChanged -= EditableUser_PropertyChanged;

            // Пересоздаем EditableUser из оригинального пользователя
            EditableUser = CloneUser(_originalUser);

            // Подписываемся на новый экземпляр
            EditableUser.PropertyChanged += EditableUser_PropertyChanged;

            IsInEditMode = true;
            HasChanges = false;
        }

        [RelayCommand]
        private void Cancel()
        {
            //if (_originalUser != null)
            //{
            //    EditableUser = CloneUser(_originalUser);
            //}

            if (_originalUser != null)
            {
                // Отписка от старого
                if (EditableUser != null)
                    EditableUser.PropertyChanged -= EditableUser_PropertyChanged;

                EditableUser = CloneUser(_originalUser);
                EditableUser.PropertyChanged += EditableUser_PropertyChanged;
            }

            IsInEditMode = false;
            HasChanges = false;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                // Загружаем пользователя из контекста EF
                var userFromDb = await _userService.GetUserByIdAsync(EditableUser.Id);
                if (userFromDb == null)
                {
                    MessageBox.Show("Пользователь не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновляем поля
                CopyUser(EditableUser, userFromDb);

                // Хешируем пароль, если задан
                if (!string.IsNullOrWhiteSpace(EditableUser.InitialPassword))
                {
                    userFromDb.SetPassword(EditableUser.InitialPassword);
                }

                await _userService.UpdateUserAsync(userFromDb);

                // Обновляем отображаемую копию
                _originalUser = CloneUser(userFromDb); // сделай поле не `readonly`
                EditableUser = CloneUser(userFromDb);

                IsInEditMode = false;
                HasChanges = false;
                EditableUser.InitialPassword = null;

                MessageBox.Show("Данные пользователя успешно обновлены.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            try
            {
                var userFromDb = await _userService.GetUserByIdAsync(EditableUser.Id);
                if (userFromDb == null)
                {
                    MessageBox.Show("Пользователь не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await _userService.DeleteUserAsync(userFromDb);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #endregion

        // Вспомогательные методы (клонирование)
        #region Клонирование/копирование

        private static CRMS.Domain.Entities.User CloneUser(CRMS.Domain.Entities.User user)
        {
            return new CRMS.Domain.Entities.User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Initials = user.Initials,
                DisplayName = user.DisplayName,
                Description = user.Description,
                Office = user.Office,
                Email = user.Email,
                WebPage = user.WebPage,
                DateOfBirth = user.DateOfBirth,
                Street = user.Street,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                Country = user.Country,
                UserLogonName = user.UserLogonName,
                Avatar = user.Avatar,
                WorkPhone = user.WorkPhone,
                MobilePhone = user.MobilePhone,
                IPPhone = user.IPPhone,
                JobTitle = user.JobTitle,
                Department = user.Department,
                Company = user.Company,
                AccountCreated = user.AccountCreated,
                Status = user.Status,
                InitialPassword = user.InitialPassword,
                ManagerName = user.ManagerName,
                //GroupMembers = user.GroupMembers?.ToList() ?? new List<GroupMember>(),
                GroupMembers = user.GroupMembers != null
                    ? new List<GroupMember>(user.GroupMembers.Select(gm => new GroupMember
                    {
                        GroupId = gm.GroupId,
                        UserId = gm.UserId,
                        Group = gm.Group
                    }))
                    : new List<GroupMember>(),
                Role = user.Role
            };
        }

        private static void CopyUser(CRMS.Domain.Entities.User source, CRMS.Domain.Entities.User target)
        {
            target.FirstName = source.FirstName;
            target.LastName = source.LastName;
            target.Initials = source.Initials;
            target.DisplayName = source.DisplayName;
            target.Description = source.Description;
            target.Office = source.Office;
            target.Email = source.Email;
            target.WebPage = source.WebPage;
            target.DateOfBirth = source.DateOfBirth;
            target.Street = source.Street;
            target.City = source.City;
            target.State = source.State;
            target.PostalCode = source.PostalCode;
            target.Country = source.Country;
            target.UserLogonName = source.UserLogonName;
            target.Avatar = source.Avatar;
            target.WorkPhone = source.WorkPhone;
            target.MobilePhone = source.MobilePhone;
            target.IPPhone = source.IPPhone;
            target.JobTitle = source.JobTitle;
            target.Department = source.Department;
            target.Company = source.Company;
            target.Status = source.Status;
            target.InitialPassword = source.InitialPassword;
            target.ManagerName = source.ManagerName;
            target.Role = source.Role;
        }
        #endregion        
    }
}
