using MahApps.Metro.Controls;
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
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using CRMS.Views.Admin.Groups;
using MaterialDesignThemes.Wpf;
using CRMS.Domain.Entities;
using CRMS.ViewModels.Admin;
using CRMS.Business.Services.UserService;

namespace CRMS.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для UserCreateWindow.xaml
    /// </summary>
    public partial class UserCreateWindow : MetroWindow
    {
        private readonly UserCreateWindowViewModel _viewModel;

        private static bool _isFormatting = false;

        public UserCreateWindow(IUserService userService)
        {
            InitializeComponent();

            _viewModel = new UserCreateWindowViewModel(userService);
            DataContext = _viewModel;
        }

        // Обработка ввода только цифр
        private void OnlyDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        // Обработка вставки текста — только цифры
        private void OnlyDigits_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, @"^\d+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnlyPhoneChars_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры, +, -, скобки и пробел
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d\+\-\(\)\s]+$");
        }

        private void OnlyPhoneChars_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, @"^[\d\+\-\(\)\s]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !Regex.IsMatch(e.Text, @"\d");
        }

        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            var digits = Regex.Replace(textBox.Text, @"\D", ""); // Убираем всё, кроме цифр

            if (digits.Length > 0 && digits[0] != '3')
                digits = "375" + digits; // Автодобавление кода страны, если пользователь не ввёл

            if (digits.Length > 12)
                digits = digits.Substring(0, 12);

            string formatted = FormatPhone(digits);

            _isFormatting = true;
            textBox.Text = formatted;
            textBox.CaretIndex = textBox.Text.Length;
            _isFormatting = false;
        }

        private string FormatPhone(string input)
        {
            if (input.Length <= 3)
                return "+" + input;
            else if (input.Length <= 5)
                return $"+{input.Substring(0, 3)} ({input.Substring(3)}";
            else if (input.Length <= 8)
                return $"+{input.Substring(0, 3)} ({input.Substring(3, 2)}) {input.Substring(5)}";
            else if (input.Length <= 10)
                return $"+{input.Substring(0, 3)} ({input.Substring(3, 2)}) {input.Substring(5, 3)}-{input.Substring(8)}";
            else
                return $"+{input.Substring(0, 3)} ({input.Substring(3, 2)}) {input.Substring(5, 3)}-{input.Substring(8, 2)}-{input.Substring(10)}";
        }

        //private async void Delete_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new ConfirmationDialog
        //    {
        //        Message = "Вы действительно хотите удалить этого пользователя?"
        //    };

        //    var result = await DialogHost.Show(dialog, "EditDeleteGroupDialogHost");
        //    if (result is bool confirmed && confirmed)
        //    {
        //        await _viewModel.DeleteAsyncCommand.ExecuteAsync(null);
        //        //MessageBox.Show("Пользователь успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
        //        Close();
        //    }
        //}

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is UserCreateWindowViewModel viewModel)
            {
                viewModel.EditableUser.InitialPassword = passwordBox.Password;
                viewModel.HasChanges = true;
            }
        }

        //private void Edit_Click(object sender, RoutedEventArgs e)
        //{
        //    _viewModel.EditCommand.Execute(null);
        //}

        //private void Cancel_Click(object sender, RoutedEventArgs e)
        //{
        //    _viewModel.CancelCommand.Execute(null);
        //}

        //private async void Save_Click(object sender, RoutedEventArgs e)
        //{
        //    await _viewModel.SaveAsyncCommand.ExecuteAsync(null);
        //}

        //private void Ok_Click(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}

        //private void SelectAvatar_Click(object sender, RoutedEventArgs e)
        //{
        //    _viewModel.SelectAvatarCommand.Execute(null);
        //}
    }
}

