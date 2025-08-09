using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace CRMS.ViewModels
{
    public partial class RichTextEditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private RichTextBox? editor;

        [RelayCommand]
        private void ToggleBold()
        {
            EditingCommands.ToggleBold.Execute(null, Editor);
        }

        [RelayCommand]
        private void ToggleItalic()
        {
            EditingCommands.ToggleItalic.Execute(null, Editor);
        }

        [RelayCommand]
        private void ToggleUnderline()
        {
            EditingCommands.ToggleUnderline.Execute(null, Editor);
        }

        [RelayCommand]
        private void AlignLeft()
        {
            EditingCommands.AlignLeft.Execute(null, Editor);
        }

        [RelayCommand]
        private void AlignCenter()
        {
            EditingCommands.AlignCenter.Execute(null, Editor);
        }

        [RelayCommand]
        private void AlignRight()
        {
            EditingCommands.AlignRight.Execute(null, Editor);
        }

        [RelayCommand]
        private void InsertImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };
            if (dlg.ShowDialog() == true)
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(dlg.FileName)),
                    Width = 200
                };

                var container = new InlineUIContainer(image);
                var caret = Editor?.CaretPosition;
                //caret?.InsertInline(container);
            }
        }

        [RelayCommand]
        private void Save()
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Rich Text Format (*.rtf)|*.rtf"
            };
            if (dlg.ShowDialog() == true && Editor != null)
            {
                using var fs = new FileStream(dlg.FileName, FileMode.Create);
                var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
                range.Save(fs, DataFormats.Rtf);
            }
        }
    }
}

