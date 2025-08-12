using CRMS.ViewModels;
using CRMS.ViewModels.UserVM;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace CRMS.Views
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class MainUserPage : Page
    {
        public MainUserPage()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<UserTicketsViewModel>();
        }
    }
}
