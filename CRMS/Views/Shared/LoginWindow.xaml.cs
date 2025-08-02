using CRMS.ViewModels;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace CRMS.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public LoginWindow(LoginWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }       
    }
}
