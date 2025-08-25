using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CRMS.Business.Services.DocumentService
{
    public class DocumentConverter : IDocumentConverter
    {
        public string FlowDocumentToHtml(FlowDocument doc)
        {
            if (doc == null) return string.Empty;

            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family: sans-serif;\">");

            foreach (var block in doc.Blocks)
                sb.Append(ConvertBlock(block));

            sb.Append("</div>");
            return sb.ToString();
        }

        private string ConvertBlock(Block block)
        {
            return block switch
            {
                Paragraph paragraph => ConvertParagraph(paragraph),
                List list => ConvertList(list),
                Table table => ConvertTable(table),
                Section section => ConvertSection(section),
                BlockUIContainer uiContainer => ConvertBlockUIContainer(uiContainer),
                _ => ""
            };
        }

        private string ConvertParagraph(Paragraph paragraph)
        {
            var inlineContent = ConvertInlines(paragraph.Inlines);

            // Стили параграфа
            var style = new StringBuilder();

            if (paragraph.Background is SolidColorBrush bgBrush)
                style.Append($"background-color:{bgBrush.Color.ToString()};");

            // Всегда используем черный цвет текста для лучшей читаемости в email
            style.Append($"color:black;");

            if (paragraph.FontSize > 0)
                style.Append($"font-size:{paragraph.FontSize}pt;");

            if (paragraph.FontFamily != null)
                style.Append($"font-family:{paragraph.FontFamily.Source};");

            // Выравнивание текста
            if (paragraph.TextAlignment != TextAlignment.Left)
            {
                string align = paragraph.TextAlignment switch
                {
                    TextAlignment.Center => "center",
                    TextAlignment.Right => "right",
                    TextAlignment.Justify => "justify",
                    _ => "left"
                };
                style.Append($"text-align:{align};");
            }

            // Проверяем, является ли параграф заголовком
            if (paragraph.FontSize >= 14 || paragraph.FontWeight == FontWeights.Bold)
            {
                var level = GetHeadingLevel(paragraph);
                return $"<h{level} style='{style}'>{inlineContent}</h{level}>";
            }

            return $"<p style='{style}'>{inlineContent}</p>";
        }

        private int GetHeadingLevel(Paragraph paragraph)
        {
            if (paragraph.FontSize >= 20) return 1;
            if (paragraph.FontSize >= 18) return 2;
            if (paragraph.FontSize >= 16) return 3;
            if (paragraph.FontSize >= 14) return 4;
            return 5;
        }

        private string ConvertList(List list)
        {
            var listTag = list.MarkerStyle == TextMarkerStyle.Disc ? "ul" : "ol";
            var sb = new StringBuilder();
            sb.Append($"<{listTag}>");

            foreach (var listItem in list.ListItems)
            {
                sb.Append("<li>");
                foreach (var block in listItem.Blocks)
                {
                    sb.Append(ConvertBlock(block));
                }
                sb.Append("</li>");
            }

            sb.Append($"</{listTag}>");
            return sb.ToString();
        }

        private string ConvertTable(Table table)
        {
            var sb = new StringBuilder();
            sb.Append("<table border='1' cellspacing='0' cellpadding='4' style='border-collapse: collapse; width: 100%;'>");

            foreach (var rowGroup in table.RowGroups)
            {
                foreach (var row in rowGroup.Rows)
                {
                    sb.Append("<tr>");
                    foreach (var cell in row.Cells)
                    {
                        sb.Append("<td style='border: 1px solid #ddd; padding: 8px;'>");
                        foreach (var block in cell.Blocks)
                        {
                            sb.Append(ConvertBlock(block));
                        }
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                }
            }

            sb.Append("</table>");
            return sb.ToString();
        }

        private string ConvertSection(Section section)
        {
            var sb = new StringBuilder();
            foreach (var block in section.Blocks)
            {
                sb.Append(ConvertBlock(block));
            }
            return sb.ToString();
        }

        private string ConvertBlockUIContainer(BlockUIContainer uiContainer)
        {
            if (uiContainer.Child is System.Windows.Controls.Image img)
            {
                // Используем выравнивание из дочернего изображения
                return $"<div style='text-align: {GetHorizontalAlignment(img.HorizontalAlignment)};'>{ConvertImage(img)}</div>";
            }
            return "";
        }

        private string GetHorizontalAlignment(HorizontalAlignment alignment)
        {
            return alignment switch
            {
                HorizontalAlignment.Left => "left",
                HorizontalAlignment.Center => "center",
                HorizontalAlignment.Right => "right",
                _ => "left"
            };
        }

        private string ConvertInlines(InlineCollection inlines)
        {
            var sb = new StringBuilder();
            foreach (var inline in inlines)
            {
                if (inline is Run run)
                {
                    var text = System.Net.WebUtility.HtmlEncode(run.Text);

                    // Форматирование текста
                    var style = new StringBuilder();

                    // Размер шрифта
                    if (run.FontSize > 0 && !double.IsNaN(run.FontSize))
                        style.Append($"font-size: {run.FontSize}pt;");

                    // Всегда используем черный цвет текста
                    style.Append($"color: black;");

                    // Жирный шрифт
                    if (run.FontWeight == FontWeights.Bold)
                        text = $"<strong>{text}</strong>";

                    // Курсив
                    if (run.FontStyle == FontStyles.Italic)
                        text = $"<em>{text}</em>";

                    // Подчеркивание
                    if (run.TextDecorations != null && run.TextDecorations.Contains(TextDecorations.Underline[0]))
                        text = $"<u>{text}</u>";

                    // Зачеркивание
                    if (run.TextDecorations != null && run.TextDecorations.Contains(TextDecorations.Strikethrough[0]))
                        text = $"<s>{text}</s>";

                    // Применяем стили
                    if (style.Length > 0)
                        text = $"<span style='{style}'>{text}</span>";

                    sb.Append(text);
                }
                else if (inline is Bold bold)
                {
                    sb.Append($"<strong>{ConvertInlines(bold.Inlines)}</strong>");
                }
                else if (inline is Italic italic)
                {
                    sb.Append($"<em>{ConvertInlines(italic.Inlines)}</em>");
                }
                else if (inline is Underline underline)
                {
                    sb.Append($"<u>{ConvertInlines(underline.Inlines)}</u>");
                }
                else if (inline is LineBreak)
                {
                    sb.Append("<br/>");
                }
                else if (inline is InlineUIContainer ui)
                {
                    if (ui.Child is System.Windows.Controls.Image img)
                    {
                        // Используем выравнивание из дочернего изображения
                        sb.Append($"<span style='display: inline-block; text-align: {GetHorizontalAlignment(img.HorizontalAlignment)};'>{ConvertImage(img)}</span>");
                    }
                }
                else if (inline is Span span)
                {
                    sb.Append(ConvertInlines(span.Inlines));
                }
                else if (inline is Hyperlink hyperlink)
                {
                    var url = hyperlink.NavigateUri?.ToString() ?? "#";
                    sb.Append($"<a href='{url}'>{ConvertInlines(hyperlink.Inlines)}</a>");
                }
            }
            return sb.ToString();
        }

        private string ConvertImage(System.Windows.Controls.Image img)
        {
            try
            {
                if (img.Source is BitmapSource bitmap)
                {
                    // Стили для изображения
                    var style = new StringBuilder();

                    // Размеры
                    if (!double.IsNaN(img.Width) && img.Width > 0)
                        style.Append($"width: {img.Width}px;");
                    if (!double.IsNaN(img.Height) && img.Height > 0)
                        style.Append($"height: {img.Height}px;");

                    // Отступы
                    if (img.Margin.Left != 0 || img.Margin.Top != 0 || img.Margin.Right != 0 || img.Margin.Bottom != 0)
                    {
                        style.Append($"margin: {img.Margin.Top}px {img.Margin.Right}px {img.Margin.Bottom}px {img.Margin.Left}px;");
                    }

                    string styleStr = style.ToString();
                    string imgTag;

                    // Проверяем, является ли источник data URI
                    if (bitmap is BitmapImage bmp && bmp.UriSource != null &&
                        bmp.UriSource.IsAbsoluteUri && bmp.UriSource.Scheme == "data")
                    {
                        imgTag = $"<img src='{bmp.UriSource.AbsoluteUri}' style='{styleStr}'/>";
                    }
                    else
                    {
                        // Конвертируем в base64
                        using var ms = new MemoryStream();
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(ms);

                        string base64 = Convert.ToBase64String(ms.ToArray());
                        imgTag = $"<img src='data:image/png;base64,{base64}' style='{styleStr}'/>";
                    }

                    return imgTag;
                }
            }
            catch
            {
                return "<div style='color: #999; font-style: italic; padding: 10px; border: 1px dashed #ccc;'>[Изображение не может быть отображено]</div>";
            }
            return "";
        }
    }
}