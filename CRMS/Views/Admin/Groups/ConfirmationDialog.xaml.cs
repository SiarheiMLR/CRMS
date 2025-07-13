using MaterialDesignThemes.Wpf;
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

namespace CRMS.Views.Admin.Groups
{
    /// <summary>
    /// Логика взаимодействия для ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : UserControl
    {
        public string Title { get; set; } = "Подтверждение";
        public string Message { get; set; } = "";
        public string YesButtonText { get; set; } = "Да";
        public string NoButtonText { get; set; } = "Нет";

        public ConfirmationDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close("MainDialogHost", true);
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close("MainDialogHost", false);
        }
    }
}
