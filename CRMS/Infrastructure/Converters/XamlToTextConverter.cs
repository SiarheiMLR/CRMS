using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class XamlToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string xamlText)
            {
                // Убираем XAML теги и оставляем только текст
                return Regex.Replace(xamlText, @"<[^>]+>", string.Empty).Trim();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}