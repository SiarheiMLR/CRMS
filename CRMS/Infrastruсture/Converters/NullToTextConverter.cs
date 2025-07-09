using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class NullToTextConverter : IValueConverter
    {
        public string NullText { get; set; } = "Не указано";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? (parameter as string ?? NullText);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
