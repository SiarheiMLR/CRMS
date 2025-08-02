using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CRMS.Domain.Entities;

namespace CRMS.Infrastructure.Converters
{
    public class UserStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                UserStatus.Active => Brushes.DarkGreen,
                UserStatus.Inactive => Brushes.DarkBlue,
                UserStatus.Suspended => Brushes.DarkRed,
                _ => Brushes.Black
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

