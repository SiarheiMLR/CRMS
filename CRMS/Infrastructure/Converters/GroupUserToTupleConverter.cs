using System;
using System.Globalization;
using System.Windows.Data;
using CRMS.Domain.Entities;

namespace CRMS.Infrastructure.Converters
{
    public class GroupUserToTupleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is Group group && values[1] is User user)
            {
                return (group, user);
            }
            return null!;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

