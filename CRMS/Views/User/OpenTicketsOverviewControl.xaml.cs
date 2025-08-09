
using System.Windows.Controls;
using CRMS.ViewModels.Admin.Groups;
using CRMS.ViewModels.UserVM;
using Microsoft.Extensions.DependencyInjection;

namespace CRMS.Views.User
{
    /// <summary>
    /// Логика взаимодействия для OpenTicketsOverviewControl.xaml
    /// </summary>
    public partial class OpenTicketsOverviewControl : UserControl
    {
        public OpenTicketsOverviewControl()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<UserTicketsViewModel>();            
        }
    }
}
