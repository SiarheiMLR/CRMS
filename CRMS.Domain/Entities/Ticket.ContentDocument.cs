using CRMS.Domain.Utilities;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Diagnostics;

namespace CRMS.Domain.Entities
{
    public partial class Ticket
    {
        /// <summary>
        /// Удобный доступ к FlowDocument, хранится в Content как XAML.
        /// </summary>
        [NotMapped] // Если используете Entity Framework
        public FlowDocument ContentDocument
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(Content))
                        return new FlowDocument();

                    return (FlowDocument)XamlReader.Parse(Content);
                }
                catch
                {
                    return new FlowDocument();
                }
            }
            set
            {
                Content = XamlWriter.Save(value);
            }
        }

        public static string ConvertFlowDocumentToXaml(FlowDocument document)
        {
            if (document == null || document.Blocks.Count == 0)
                return string.Empty;

            try
            {
                // Используем XamlWriter для сохранения всего документа с изображениями
                return XamlWriter.Save(document);
            }
            catch
            {
                // Если не удалось сохранить как XAML, сохраняем как текст
                return new TextRange(document.ContentStart, document.ContentEnd).Text;
            }
        }

        public static FlowDocument ConvertXamlToFlowDocument(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new FlowDocument();

            try
            {
                // Используем XamlReader для загрузки документа с изображениями
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    return (FlowDocument)XamlReader.Load(stream);
                }
            }
            catch
            {
                // Если не удалось загрузить как XAML, возвращаем как текст
                return CreateTextFlowDocument(content);
            }
        }

        private static bool IsValidXaml(string content) =>
            !string.IsNullOrWhiteSpace(content) && content.TrimStart().StartsWith("<");

        private static FlowDocument CreateTextFlowDocument(string text)
        {
            var document = new FlowDocument();
            document.Blocks.Add(new Paragraph(new Run(text)));
            return document;
        }

        /// <summary>
        /// Возвращает FlowDocument, где изображения из вложений подставлены вместо плейсхолдеров.
        /// </summary>
        public FlowDocument GetContentDocumentWithAttachments()
        {
            var doc = ContentDocument;

            if (doc == null || Attachments == null || Attachments.Count == 0)
                return doc;

            // Ищем все изображения в документе
            var images = FindImagesInFlowDocument(doc);

            foreach (var image in images)
            {
                // Если это плейсхолдер с указанием имени файла в Tag
                if (IsPlaceholderImage(image) && image.Tag is string fileName)
                {
                    // Ищем соответствующее вложение
                    var attachment = Attachments.FirstOrDefault(a =>
                        a.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

                    if (attachment != null)
                    {
                        // Заменяем плейсхолдер на реальное изображение
                        ReplacePlaceholderWithAttachment(image, attachment);
                    }
                }
                // Обрабатываем data URI изображения (из буфера обмена)
                else if (image.Source is BitmapImage bmp && bmp.UriSource != null &&
                         bmp.UriSource.ToString().StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    // Data URI изображения уже встроены в XAML, ничего не делаем
                }
                // Обрабатываем file:/// ссылки (должны были быть заменены на плейсхолдеры)
                else if (image.Source is BitmapImage bmp2 && bmp2.UriSource != null &&
                         bmp2.UriSource.ToString().StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                {
                    // Заменяем file:/// ссылки на плейсхолдеры
                    ReplaceWithPlaceholder(image);
                }
            }

            return doc;
        }

        private static void ReplaceWithPlaceholder(Image image)
        {
            try
            {
                var placeholder = new BitmapImage();
                placeholder.BeginInit();
                placeholder.UriSource = new Uri("pack://application:,,,/CRMS;component/Images/placeholder.png");
                placeholder.CacheOption = BitmapCacheOption.OnLoad;
                placeholder.EndInit();

                image.Source = placeholder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка создания плейсхолдера: {ex.Message}");
            }
        }

        private static bool IsPlaceholderImage(Image image)
        {
            if (image.Source is BitmapImage bmp && bmp.UriSource != null)
            {
                return bmp.UriSource.ToString().Contains("placeholder.png", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private static void ReplacePlaceholderWithAttachment(Image image, Attachment attachment)
        {
            try
            {
                using var ms = new MemoryStream(attachment.FileData);
                var newImage = new BitmapImage();
                newImage.BeginInit();
                newImage.CacheOption = BitmapCacheOption.OnLoad;
                newImage.StreamSource = ms;
                newImage.EndInit();

                image.Source = newImage;
                image.Tag = null; // Очищаем Tag после восстановления
            }
            catch (Exception ex)
            {
                // Логируем ошибку для отладки
                Debug.WriteLine($"Ошибка обработки изображения: {ex.Message}");
                // В продакшн-коде лучше использовать полноценное логирование
                // Logger.Error(ex, "Ошибка обработки изображения");

                // Оставляем плейсхолдер, чтобы не ломать весь документ
            }
        }

        public static IEnumerable<Image> FindImagesInFlowDocument(FlowDocument document)
        {
            var images = new List<Image>();
            if (document == null) return images;

            foreach (var block in document.Blocks)
            {
                FindImagesInBlock(block, images);
            }
            return images;
        }

        private static void FindImagesInBlock(Block block, List<Image> images)
        {
            if (block is Paragraph paragraph)
            {
                foreach (var inline in paragraph.Inlines)
                {
                    FindImagesInInline(inline, images);
                }
            }
            else if (block is List list)
            {
                foreach (var listItem in list.ListItems)
                {
                    foreach (var subBlock in listItem.Blocks)
                    {
                        FindImagesInBlock(subBlock, images);
                    }
                }
            }
            else if (block is Table table)
            {
                foreach (var rowGroup in table.RowGroups)
                {
                    foreach (var row in rowGroup.Rows)
                    {
                        foreach (var cell in row.Cells)
                        {
                            foreach (var subBlock in cell.Blocks)
                            {
                                FindImagesInBlock(subBlock, images);
                            }
                        }
                    }
                }
            }
            else if (block is Section section)
            {
                foreach (var subBlock in section.Blocks)
                {
                    FindImagesInBlock(subBlock, images);
                }
            }
            else if (block is BlockUIContainer uiContainer)
            {
                if (uiContainer.Child is Image image)
                {
                    images.Add(image);
                }
            }
        }

        private static void FindImagesInInline(Inline inline, List<Image> images)
        {
            if (inline is InlineUIContainer uiContainer)
            {
                if (uiContainer.Child is Image image)
                {
                    images.Add(image);
                }
            }
            else if (inline is Span span)
            {
                foreach (var childInline in span.Inlines)
                {
                    FindImagesInInline(childInline, images);
                }
            }
        }

        private static void FindImagesInDocument(DependencyObject parent, List<Image> images)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Image image)
                {
                    images.Add(image);
                }
                else
                {
                    FindImagesInDocument(child, images);
                }
            }
        }

        // Сохраняем оригинальный метод для возможного использования в других частях кода
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
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