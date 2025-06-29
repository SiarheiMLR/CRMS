using CRMS.ViewModels.UserVM;
using System.Windows.Controls;


namespace CRMS.Views.User.TicketsPage
{
    /// <summary>
    /// Логика взаимодействия для UserTicketsPage.xaml
    /// </summary>
    public partial class UserTicketsPage : Page
    {
        public UserTicketsPage(UserTicketsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
