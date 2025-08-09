using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using CRMS.ViewModels;

namespace CRMS.Views.Shared
{
    public partial class RichTextEditor : UserControl
    {
        public RichTextEditor()
        {
            InitializeComponent();
            this.Loaded += RichTextEditor_Loaded;
        }

        private void RichTextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RichTextEditorViewModel viewModel)
            {
                viewModel.Editor = Editor; // Привязка RichTextBox к VM
            }
        }
    }
}
