using System;
using System.IO;
using System.Text;
using System.Xml;

namespace CRMS.Business.Services.DocumentService
{
    /// <summary>
    /// Конвертирует XAML (FlowDocument) в HTML.
    /// Код основан на примерах Microsoft (MSDN Samples).
    /// </summary>
    public static class HtmlFromXamlConverter
    {
        public static string ConvertXamlToHtml(string xamlString)
        {
            if (string.IsNullOrEmpty(xamlString))
                return string.Empty;

            try
            {
                using (var stringReader = new StringReader(xamlString))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    var html = new StringBuilder();
                    html.Append("<html><body>");

                    while (xmlReader.Read())
                    {
                        switch (xmlReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                html.Append(GetHtmlStartTag(xmlReader));
                                if (xmlReader.IsEmptyElement)
                                    html.Append(GetHtmlEndTag(xmlReader));
                                break;

                            case XmlNodeType.EndElement:
                                html.Append(GetHtmlEndTag(xmlReader));
                                break;

                            case XmlNodeType.Text:
                                html.Append(System.Net.WebUtility.HtmlEncode(xmlReader.Value));
                                break;
                        }
                    }

                    html.Append("</body></html>");
                    return html.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"<html><body><p>Ошибка конвертации XAML → HTML: {ex.Message}</p></body></html>";
            }
        }

        private static string GetHtmlStartTag(XmlReader reader)
        {
            string tag = reader.Name switch
            {
                "Paragraph" => "<p>",
                "Run" => "",
                "Bold" => "<b>",
                "Italic" => "<i>",
                "Underline" => "<u>",
                "LineBreak" => "<br/>",
                "Span" => "<span>",
                "List" => "<ul>",
                "ListItem" => "<li>",
                "Table" => "<table border='1'>",
                "TableRowGroup" => "",
                "TableRow" => "<tr>",
                "TableCell" => "<td>",
                "Image" => "<img src='[Image]'/>", // пока заглушка для картинок
                _ => ""
            };

            return tag;
        }

        private static string GetHtmlEndTag(XmlReader reader)
        {
            string tag = reader.Name switch
            {
                "Paragraph" => "</p>",
                "Bold" => "</b>",
                "Italic" => "</i>",
                "Underline" => "</u>",
                "Span" => "</span>",
                "List" => "</ul>",
                "ListItem" => "</li>",
                "Table" => "</table>",
                "TableRow" => "</tr>",
                "TableCell" => "</td>",
                _ => ""
            };

            return tag;
        }
    }
}

