using CRMS.Domain.Entities;
using CRMS.ViewModels.Faq;
using System.Windows.Controls;
using System.Windows.Input;

namespace CRMS.Views.Faq
{
    public partial class FaqPage : Page
    {
        public FaqPage()
        {
            InitializeComponent();
        }

        private void FaqItem_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is FaqItem item)
            {
                var vm = (FaqPageViewModel)DataContext;
                vm.NavigateToDetail(item);
            }
        }
    }
}
