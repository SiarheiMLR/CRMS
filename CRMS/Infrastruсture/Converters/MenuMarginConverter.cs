using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class MenuMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isMenuOpen = value is bool b && b;
            return isMenuOpen ? new Thickness(300, 0, 0, 0) : new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

