using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.ActiveDirectoryService;
using CRMS.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CRMS.Views;

namespace CRMS.ViewModels.AD
{
    public partial class ADLoginWindowViewModel : ObservableObject
    {
        private readonly IActiveDirectoryService _adService;

        public ADLoginWindowViewModel()
        {
            _adService = new ActiveDirectoryService();
        }

        [ObservableProperty]
        private string login = string.Empty;

        public string Password { get; set; } = string.Empty;

        public Action? CloseAction { get; set; }

        [RelayCommand]
        private async Task AuthorizeAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Login) || !Login.Contains("@"))
                {
                    MessageBox.Show("Введите логин в формате user@domain.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string domain = Login.Split('@')[1];

                List<ADUserDto> users = await _adService.GetAllUsersAsync(domain, Login, Password);

                if (users.Count == 0)
                {
                    MessageBox.Show("Пользователи не найдены.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Навигация во фрейме на UserListPage
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    mainWindow?.MainFrame.Navigate(new Views.Admin.UserListPage(users));
                });

                CloseAction?.Invoke();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Неверный логин или пароль администратора домена.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к Active Directory:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke();
        }
    }
}
