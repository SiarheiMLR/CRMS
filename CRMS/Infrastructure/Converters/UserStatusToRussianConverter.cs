using CRMS.Domain.Entities;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class UserStatusToRussianConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                UserStatus.Active => "Активен",
                UserStatus.Inactive => "Неактивен",
                UserStatus.Suspended => "Заблокирован",
                _ => "Неизвестно"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                "Активен" => UserStatus.Active,
                "Неактивен" => UserStatus.Inactive,
                "Заблокирован" => UserStatus.Suspended,
                _ => UserStatus.Inactive
            };
        }
    }
}
