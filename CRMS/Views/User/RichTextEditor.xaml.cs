using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Resources;

namespace CRMS.Views.User
{
    public partial class RichTextEditor : UserControl
    {
        public static FlowDocument DefaultDocument { get; } = new FlowDocument();

        public class EmojiItem
        {
            public string Unicode { get; set; }
            public string ImagePath { get; set; }
        }

        public ObservableCollection<EmojiItem> Emojis { get; } = new();

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register(
                "Document",
                typeof(FlowDocument),
                typeof(RichTextEditor),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnDocumentChanged));

        public static readonly DependencyProperty SaveCommandProperty =
            DependencyProperty.Register(
                "SaveCommand",
                typeof(ICommand),
                typeof(RichTextEditor),
                new PropertyMetadata(null));

        public FlowDocument Document
        {
            get => (FlowDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        private static void OnDocumentChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextEditor editor)
            {
                editor.Editor.Document = e.NewValue as FlowDocument ?? new FlowDocument();
            }
        }

        public ObservableCollection<double> FontSizes { get; } =
            new ObservableCollection<double> { 8, 10, 12, 14, 16, 18, 20, 22, 24, 28, 36, 48, 72 };

        public double SelectedFontSize
        {
            get => (double)GetValue(SelectedFontSizeProperty);
            set => SetValue(SelectedFontSizeProperty, value);
        }

        public static readonly DependencyProperty SelectedFontSizeProperty =
            DependencyProperty.Register("SelectedFontSize", typeof(double), typeof(RichTextEditor),
            new PropertyMetadata(12.0));        

        public RichTextEditor()
        {
            InitializeComponent();
            SelectedFontSize = 12;

            // Загрузка эмодзи
            LoadEmojis();
        }

        private static readonly Dictionary<string, BitmapImage> EmojiCache = new();

        private BitmapImage LoadEmojiImage(string path)
        {
            if (EmojiCache.TryGetValue(path, out var image))
                return image;

            image = new BitmapImage(new Uri(path));
            EmojiCache[path] = image;
            return image;
        }
        
        private void LoadEmojis()
        {
            Emojis.Clear();

            string outFolder = Path.Combine(AppContext.BaseDirectory ?? ".", "Resources", "Emojis");

            if (Directory.Exists(outFolder))
            {
                // Если файлы скопированы в выходную папку (Build Action = Content, Copy to Output Directory = Always/IfNewer)
                foreach (var file in Directory.GetFiles(outFolder, "*.png"))
                {
                    TryAddEmojiFromFile(Path.GetFileName(file));
                }
                return;
            }

            // Фоллбек: если файлы встроены как WPF Resource (Build Action = Resource),
            // они попадают в [AssemblyName].g.resources и мы можем перечислить их имена.
            var asm = Assembly.GetExecutingAssembly();
            string gResourceName = asm.GetName().Name + ".g.resources";

            using (var stream = asm.GetManifestResourceStream(gResourceName))
            {
                if (stream == null)
                {
                    // Ничего не найдено — логирование/обработка
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine($"Не найден {gResourceName}. Проверьте Build Action и путь к ресурсам.");
                    #endif
                    return;
                }

                using (var rr = new ResourceReader(stream))
                {
                    foreach (DictionaryEntry entry in rr)
                    {
                        string resourceKey = (string)entry.Key; // например "resources/emojis/1f600.png"
                                                                // Нормализуем и ищем папку resources/emojis
                        if (resourceKey.StartsWith("resources/emojis/", StringComparison.OrdinalIgnoreCase))
                        {
                            string fileName = Path.GetFileName(resourceKey);
                            TryAddEmojiFromFile(fileName);
                        }
                    }
                }
            }
        }

        private void TryAddEmojiFromFile(string fileName)
        {
            try
            {
                // ожидаем имя вида "1f600.png"
                var name = Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrWhiteSpace(name))
                    return;

                if (!int.TryParse(name, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cp))
                    return;

                string emoji = char.ConvertFromUtf32(cp);

                // Формируем pack uri. Замените CRMS на фактическое имя сборки если нужно.
                string imagePath = $"pack://application:,,,/CRMS;component/Resources/Emojis/{fileName}";

                Emojis.Add(new EmojiItem { Unicode = emoji, ImagePath = imagePath });
            }
            catch (Exception ex)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении {fileName}: {ex}");
                #endif
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && Editor != null)
            {
                double fontSize = (double)e.AddedItems[0];

                // Применяем размер шрифта только к выделенному тексту
                Editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);

                // Устанавливаем размер шрифта для последующего ввода
                var newRun = new Run
                {
                    FontSize = fontSize
                };

                var caretPosition = Editor.CaretPosition;
                var paragraph = caretPosition.Paragraph;

                if (paragraph != null)
                {
                    paragraph.Inlines.Add(newRun);
                }
                else
                {
                    var newParagraph = new Paragraph(newRun);
                    Editor.Document.Blocks.Add(newParagraph);
                }

                Editor.CaretPosition = newRun.ContentEnd;
                Editor.Focus();
            }
        }

        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EmojiItem emoji)
            {
                var caretPosition = Editor.CaretPosition;

                // Создаем изображение
                var image = new Image
                {
                    Source = LoadEmojiImage(emoji.ImagePath),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(10, 0, 0, 0)
                };

                var container = new InlineUIContainer(image, caretPosition);

                var paragraph = caretPosition.Paragraph;
                if (paragraph != null)
                {
                    paragraph.Inlines.Add(container);
                }
                else
                {
                    var newParagraph = new Paragraph(container);
                    Editor.Document.Blocks.Add(newParagraph);
                }

                Editor.CaretPosition = container.ElementEnd;
                Editor.Focus();

                // Закрываем Popup после вставки
                if (FindName("EmojiToggleButton") is ToggleButton toggleButton)
                {
                    toggleButton.IsChecked = false;
                }
            }
        }        

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (FindName("EmojiToggleButton") is ToggleButton { IsChecked: true } toggleButton)
            {
                var popup = FindName("EmojiPopup") as Popup;
                if (popup != null && !popup.IsMouseOver)
                {
                    toggleButton.IsChecked = false;
                }
            }
        }

        private void InsertImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(dlg.FileName);
                    bitmap.EndInit();

                    var image = new Image
                    {
                        Source = bitmap,
                        Width = 200,
                        Stretch = Stretch.Uniform
                    };

                    var container = new InlineUIContainer(image, Editor.CaretPosition);
                    Editor.CaretPosition = container.ElementEnd;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }       
    }
}
