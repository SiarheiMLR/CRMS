using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace CRMS.Infrastructure.Converters
{
    public class XamlToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string xamlText)
            {
                try
                {
                    using (var stringReader = new StringReader(xamlText))
                    using (var xmlReader = XmlReader.Create(stringReader))
                    {
                        return XamlReader.Load(xmlReader) as FlowDocument;
                    }
                }
                catch (Exception ex)
                {
                    return CreateErrorDocument("Ошибка загрузки содержимого заявки");
                }
            }
            return new FlowDocument();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private FlowDocument CreateErrorDocument(string message)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph(new Run(message))
            {
                Foreground = Brushes.Red,
                FontStyle = FontStyles.Italic
            };
            doc.Blocks.Add(paragraph);
            return doc;
        }
    }
}