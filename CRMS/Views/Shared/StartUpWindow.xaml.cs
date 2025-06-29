using CRMS.ViewModels;
using MahApps.Metro.Controls;

namespace CRMS.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartUpWindow : MetroWindow
    {
        public StartUpWindow(StartUpWindowViewModel viewModel)
        {
            InitializeComponent();

            // Установите DataContext через параметры конструктора
            DataContext = viewModel;
        }

    }
}