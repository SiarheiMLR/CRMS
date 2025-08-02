using System.Windows;
using CRMS.ViewModels;
using MahApps.Metro.Controls;

namespace CRMS.Views.Dialogs
{
    public partial class PasswordResetSentWindow : MetroWindow
    {
        public PasswordResetSentWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
