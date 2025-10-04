using CRMS.ViewModels.Support;
using CRMS.ViewModels.UserVM;
using Microsoft.Extensions.DependencyInjection;
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
    /// Логика взаимодействия для TicketStatsControl.xaml
    /// </summary>
    public partial class TicketStatsControl : UserControl
    {
        public TicketStatsControl()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<SupportStatsViewModel>();
        }
    }
}