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
using Microsoft.Win32;
using System.IO;

namespace CRMS.Views.User
{
    /// <summary>
    /// Логика взаимодействия для CreateTicketControl.xaml
    /// </summary>
    public partial class CreateTicketControl : UserControl
    {
        public CreateTicketControl()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<UserTicketsViewModel>();
        }

        private void AttachmentsDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (DataContext is CRMS.ViewModels.UserVM.UserTicketsViewModel vm)
                    vm.AddFilesAsync(files);
            }
        }

        private void AttachmentsClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Все файлы (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                if (DataContext is CRMS.ViewModels.UserVM.UserTicketsViewModel vm)
                    vm.AddFilesAsync(dlg.FileNames);
            }
        }
    }
}
