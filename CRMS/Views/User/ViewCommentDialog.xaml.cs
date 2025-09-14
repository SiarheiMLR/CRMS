using CRMS.ViewModels.UserVM;
using CRMS.Views.User;
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

namespace CRMS.Views.User
{
    /// <summary>
    /// Логика взаимодействия для ViewCommentDialog.xaml
    /// </summary>
    public partial class ViewCommentDialog : UserControl
    {
        public ViewCommentDialog()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<UserTicketsViewModel>();
        }
    }
}
