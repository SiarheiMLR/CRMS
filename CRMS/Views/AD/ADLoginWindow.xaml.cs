using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CRMS.ViewModels.AD;
using MahApps.Metro.Controls;
using System.ComponentModel;

namespace CRMS.Views.AD
{
    public partial class ADLoginWindow : MetroWindow
    {
        public ADLoginWindow()
        {
            InitializeComponent();
            var viewModel = new ADLoginWindowViewModel();
            this.DataContext = viewModel;
            viewModel.CloseAction = Close;

            // Установка фокуса на логин при загрузке
            Loaded += (s, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ADLoginWindowViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
    }

    public class EmailValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value is string email && !string.IsNullOrWhiteSpace(email) && email.Contains("@"))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, "Некорректный email");
        }
    }
}