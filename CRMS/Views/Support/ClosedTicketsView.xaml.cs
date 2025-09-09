using CRMS.ViewModels.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CRMS.Views.Support
{
    /// <summary>
    /// Логика взаимодействия для ClosedTicketsView.xaml
    /// </summary>
    public partial class ClosedTicketsView : UserControl
    {
        public ClosedTicketsView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext is SupportTicketsViewModel vm && Tag is SupportTicketsViewModel.ViewType type)
                    vm.CurrentView = type;
            };
        }
    }
}
