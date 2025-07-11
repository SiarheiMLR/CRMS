using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CRMS.Infrastruсture.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public string TrueText { get; set; } = "Да";
        public string FalseText { get; set; } = "Нет";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? TrueText : FalseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
