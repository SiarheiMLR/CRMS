using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CRMS.Domain.Entities;

namespace CRMS.Infrastructure.Converters
{
    public class UserRoleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not User user)
                return Brushes.Black;

            // Проверяем состоит ли пользователь в группах
            bool inAnyGroup = user.GroupMembers.Any();

            if (!inAnyGroup)
                return Brushes.Black;

            return user.Role switch
            {
                UserRole.Admin => Brushes.Red,
                UserRole.Support => Brushes.SteelBlue,
                UserRole.User => Brushes.Green,
                _ => Brushes.Black
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}