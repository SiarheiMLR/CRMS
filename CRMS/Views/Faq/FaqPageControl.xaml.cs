using CRMS.Domain.Entities;
using CRMS.ViewModels.Faq;
using CRMS.ViewModels.UserVM;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace CRMS.Views.Faq
{
    public partial class FaqPageControl : UserControl
    {
        public FaqPageControl()
        {
            InitializeComponent();

            // Установка ViewModel через DI в конструкторе по умолчанию
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = App.ServiceProvider.GetRequiredService<FaqPageViewModel>();
            }
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
