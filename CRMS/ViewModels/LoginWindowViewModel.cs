using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Domain.Entities;
using System.Windows;
using CRMS.Views;
using CRMS.Business.Services.AuthService;
using Microsoft.Extensions.DependencyInjection;
using CRMS.Services;
using CRMS.Views.Admin;

namespace CRMS.ViewModels
{
    public partial class LoginWindowViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        public LoginWindowViewModel(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("Введите email и пароль!", "Аутентификация пользователя", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Аутентификация пользователя
                var user = await _authService.AuthenticateAsync(Email, Password);

                if (user != null)
                {
                    _serviceProvider.GetRequiredService<MainWindow>().Show();
                    Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()?.Close();
                    Application.Current.Windows.OfType<StartUpWindow>().FirstOrDefault()?.Close();
                }
                else
                {
                    MessageBox.Show("Введен неверный email и/или пароль.", "Аутентификация пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }        

        // Логика для сценария "Забыли пароль?" - !!! Необходимо изменить!!!!
        [RelayCommand]
        public void ForgotPassword()
        {
            Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()?.Close();
        }

        [RelayCommand]
        public void Close()
        {
            Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()?.Close();
        }
    }
}

