using System.Windows.Controls;
using CRMS.ViewModels.Admin;
using Microsoft.Extensions.DependencyInjection;
using CRMS;

namespace CRMS.Views.Admin
{
    public partial class UsersOverviewPage : Page
    {
        public UsersOverviewPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<UsersOverviewPageViewModel>();
            DataContext = viewModel;
        }
    }
}

