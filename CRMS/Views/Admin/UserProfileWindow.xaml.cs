using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CRMS.Views.Admin.Groups;
using MaterialDesignThemes.Wpf;

namespace CRMS.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для UserProfileWindow.xaml
    /// </summary>
    public partial class UserProfileWindow : MetroWindow
    {
        public UserProfileWindow(CRMS.Domain.Entities.User user)
        {
            InitializeComponent();
            DataContext = user;           
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfirmationDialog
            {
                Message = "Вы действительно хотите удалить этого пользователя?"
            };

            var result = await DialogHost.Show(dialog, "EditDeleteGroupDialogHost");
            if (result is bool confirmed && confirmed)
            {
                // TODO: Логика удаления пользователя из базы данных
                MessageBox.Show("Пользователь успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // Переход к режиму редактирования
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Отмена изменений
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение изменений
        }
    }
}
