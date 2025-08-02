using System.Windows;
using CRMS.ViewModels;
using MahApps.Metro.Controls;

namespace CRMS.Views.Dialogs
{
    public partial class ForgotPasswordWindow : MetroWindow
    {
        public string EnteredEmail { get; private set; }

        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            EnteredEmail = EmailTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(EnteredEmail))
            {
                MessageBox.Show("Пожалуйста, введите email.", "Восстановление пароля", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

