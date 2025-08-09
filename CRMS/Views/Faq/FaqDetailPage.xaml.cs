using CRMS.ViewModels.Faq;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace CRMS.Views.Faq
{
    /// <summary>
    /// Логика взаимодействия для FaqDetailPage.xaml
    /// </summary>
    public partial class FaqDetailPage : Page
    {
        public FaqDetailPage(FaqDetailPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
        }
    }
}
