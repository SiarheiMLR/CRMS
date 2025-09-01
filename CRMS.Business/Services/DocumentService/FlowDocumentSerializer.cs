using System;
using System.IO;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Windows;
using System.Windows.Markup;

namespace CRMS.Business.Services.DocumentService
{
    public class FlowDocumentSerializer
    {
        /// <summary>
        /// Сериализует FlowDocument в XAML-строку, конвертируя картинки в base64.
        /// </summary>
        public string Serialize(FlowDocument document)
        {
            if (document == null) return string.Empty;

            // Создаем копию документа для работы
            var docCopy = CloneFlowDocument(document);

            // Заменяем изображения на base64
            ReplaceImagesWithBase64(docCopy);

            // Сериализуем документ
            return XamlWriter.Save(docCopy);
        }

        /// <summary>
        /// Десериализует XAML-строку обратно в FlowDocument, восстанавливая картинки из base64.
        /// </summary>
        public FlowDocument Deserialize(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return new FlowDocument();

            try
            {
                // Десериализуем документ
                var doc = (FlowDocument)XamlReader.Parse(xaml);

                // Восстанавливаем изображения из base64
                RestoreBase64Images(doc);

                return doc;
            }
            catch
            {
                return new FlowDocument(new Paragraph(new Run("[Ошибка восстановления документа]")));
            }
        }

        /// <summary>
        /// Создает глубокую копию FlowDocument
        /// </summary>
        private FlowDocument CloneFlowDocument(FlowDocument original)
        {
            var xamlString = XamlWriter.Save(original);
            return (FlowDocument)XamlReader.Parse(xamlString);
        }

        /// <summary>
        /// Заменяет картинки на base64-строки в Source.
        /// </summary>
        private void ReplaceImagesWithBase64(FlowDocument doc)
        {
            var images = FindVisualChildren<Image>(doc);

            foreach (var img in images)
            {
                if (img.Source is BitmapSource bmp)
                {
                    try
                    {
                        using var ms = new MemoryStream();
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                        encoder.Save(ms);

                        string base64 = Convert.ToBase64String(ms.ToArray());
                        // Сохраняем base64 прямо в Source как data URI
                        img.Source = new BitmapImage(new Uri($"data:image/png;base64,{base64}"));
                    }
                    catch
                    {
                        // В случае ошибки оставляем изображение как есть
                    }
                }
            }
        }

        /// <summary>
        /// Восстанавливает картинки из base64-строк.
        /// </summary>
        private void RestoreBase64Images(FlowDocument doc)
        {
            var images = FindVisualChildren<Image>(doc);

            foreach (var img in images)
            {
                if (img.Source is BitmapImage bmp && bmp.UriSource != null &&
                    bmp.UriSource.IsAbsoluteUri && bmp.UriSource.Scheme == "data")
                {
                    try
                    {
                        string dataUri = bmp.UriSource.AbsoluteUri;
                        string base64 = dataUri.Substring(dataUri.IndexOf(",") + 1);
                        byte[] bytes = Convert.FromBase64String(base64);

                        using var ms = new MemoryStream(bytes);
                        var newBmp = new BitmapImage();
                        newBmp.BeginInit();
                        newBmp.CacheOption = BitmapCacheOption.OnLoad;
                        newBmp.StreamSource = ms;
                        newBmp.EndInit();
                        newBmp.Freeze();

                        img.Source = newBmp;
                    }
                    catch
                    {
                        // В случае ошибки заменяем на плейсхолдер
                        img.Source = CreatePlaceholderImage();
                    }
                }
            }
        }

        /// <summary>
        /// Создает изображение-плейсхолдер
        /// </summary>
        private BitmapImage CreatePlaceholderImage()
        {
            try
            {
                // Создаем простое изображение-плейсхолдер программно
                var drawingVisual = new DrawingVisual();
                using (var drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(Brushes.LightGray, new Pen(Brushes.Gray, 1),
                        new Rect(0, 0, 100, 100));
                    drawingContext.DrawText(
                        new FormattedText("Image",
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            12,
                            Brushes.Gray),
                        new Point(30, 40));
                }

                var bitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(drawingVisual);

                var bitmapImage = new BitmapImage();
                using var stream = new MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(stream.ToArray());
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Находит все элементы определенного типа в визуальном дереве
        /// </summary>
        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T tChild)
                        yield return tChild;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }
}