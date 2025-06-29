using CRMS.ViewModels.Support;
using System.Windows.Controls;

namespace CRMS.Views.Support
{
    /// <summary>
    /// Логика взаимодействия для SupportTicketsPage.xaml
    /// </summary>
    public partial class SupportTicketsPage : Page
    {
        public SupportTicketsPage(SupportTicketsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
