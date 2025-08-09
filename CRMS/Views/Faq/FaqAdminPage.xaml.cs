using CRMS.ViewModels.Faq;
using System.Windows.Controls;

namespace CRMS.Views.Faq
{
    /// <summary>
    /// Логика взаимодействия для FaqAdminPage.xaml
    /// </summary>
    public partial class FaqAdminPage : Page
    {
        public FaqAdminPage(FaqAdminPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
