using System;
using System.Globalization;
using System.Windows.Data;
using CRMS.Domain.Entities;

namespace CRMS.Infrastructure.Converters
{
    public class UserRoleEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                UserRole.Admin => "Администратор",
                UserRole.Support => "Служба поддержки",
                UserRole.User => "Пользователь",
                _ => "Не указано"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                "Администратор" => UserRole.Admin,
                "Служба поддержки" => UserRole.Support,
                "Пользователь" => UserRole.User,
                _ => UserRole.User
            };
        }
    }
}

