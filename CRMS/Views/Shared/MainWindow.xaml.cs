using CRMS.Business.Services.AuthService;
using CRMS.Domain.Entities;
using CRMS.Services;
using CRMS.ViewModels;
using CRMS.Views.Admin;
using CRMS.Views.Support;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Navigation;

namespace CRMS.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(MainWindowViewModel viewModel, 
            IAuthService authService, INavigationService navigationService)
        {
            InitializeComponent();
            DataContext = viewModel;
            navigationService.SetFrame(MainFrame);
            switch (authService.CurrentUser.Role)
            {
                case (UserRole.Admin):
                    navigationService.NavigateTo<MainAdminPage>();
                    break;
                case (UserRole.Support):
                    navigationService.NavigateTo<MainSupportPage>();
                    break;
                case (UserRole.User):
                    navigationService.NavigateTo<MainUserPage>();
                    break;
            }
            
        }
    }
}
