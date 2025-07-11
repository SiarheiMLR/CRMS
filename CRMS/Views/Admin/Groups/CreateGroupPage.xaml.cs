using CRMS.ViewModels.Admin.Groups;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace CRMS.Views.Admin.Groups
{
    /// <summary>
    /// Логика взаимодействия для CreateGroupPage.xaml
    /// </summary>
    public partial class CreateGroupPage : Page
    {
        public CreateGroupPage()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<CreateGroupViewModel>();
        }
    }
}

