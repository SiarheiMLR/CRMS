using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;

namespace CRMS.Infrastructure.Converters
{
    public class FlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDocument doc) return doc;
            return new FlowDocument();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
