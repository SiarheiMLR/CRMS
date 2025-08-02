using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Domain.Entities;
using System.Windows;
using CRMS.Views;
using CRMS.Business.Services.AuthService;
using Microsoft.Extensions.DependencyInjection;
using CRMS.Services;
using CRMS.Views.Admin;
using CRMS.Views.Dialogs;
using CRMS.Business.Services.EmailService;
using CRMS.Business.Services.EmailService.Templates;

namespace CRMS.ViewModels
{
    public partial class LoginWindowViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailService _emailService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        public LoginWindowViewModel(IAuthService authService, IServiceProvider serviceProvider, IEmailService emailService)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            _emailService = emailService;
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
                    // Проверка статуса пользователя
                    if (user.Status != UserStatus.Active)
                    {
                        var admin = await _authService.GetFirstAdminAsync();
                        string adminInfo = admin != null
                            ? $"email: {admin.Email}\nтелефон: {admin.WorkPhone}\n         {admin.MobilePhone}"
                            : "email: admin@unknown\nтелефон: не указан";

                        string statusText = user.Status switch
                        {
                            UserStatus.Inactive => "Неактивен",
                            UserStatus.Suspended => "Заблокирован",
                            _ => user.Status.ToString()
                        };

                        string message = $"В настоящее время {user.DisplayName} не может войти в систему CRMS!\n" +
                                         $"Ваш текущий статус: \"{statusText}\".\n\n" +
                                         $"Дополнительную информацию можно получить у администратора:\n{adminInfo}";

                        MessageBox.Show(message, "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

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

        // Логика для сценария "Забыли пароль?"
        [RelayCommand]
        public void ForgotPassword()
        {
            _ = ForgotPasswordAsync();
        }                
        
        private async Task ForgotPasswordAsync()
        {
            var dialog = new ForgotPasswordWindow();
            if (dialog.ShowDialog() == true)
            {
                string userEmail = dialog.EnteredEmail?.Trim();

                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    MessageBox.Show("Email не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем первого администратора
                var admin = await _authService.GetFirstAdminAsync();

                if (admin == null)
                {
                    MessageBox.Show("Не найден администратор для отправки запроса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем информацию о пользователе для DisplayName
                var user = await _authService.GetUserByEmailAsync(userEmail);
                string displayName = user != null ? user.DisplayName : userEmail;

                var parameters = new Dictionary<string, string>
                {
                    ["DisplayName"] = displayName,
                    ["Email"] = userEmail,
                    ["RequestDate"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                };

                try
                {
                    await _emailService.SendTemplateAsync(
                        to: admin.Email,
                        subject: "CRMS: Запрос на восстановление пароля",
                        template: Templates.ForgotPassword,
                        parameters: parameters
                    );

                    var confirmation = new PasswordResetSentWindow();
                    confirmation.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке письма: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public void Close()
        {
            Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()?.Close();
        }
    }
}

