using System;
using System.Globalization;
using System.Windows.Data;
using CRMS.Domain.Entities;

namespace CRMS.Infrastructure.Converters
{
    public class UserRoleToGroupLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not User user)
                return "Группа не назначена";

            // Проверяем состоит ли пользователь в группах
            bool inAnyGroup = user.GroupMembers.Any();

            if (!inAnyGroup)
                return "Группа не назначена";

            return user.Role switch
            {
                UserRole.Admin => "Администраторы системы",
                UserRole.Support => "Служба поддержки",
                UserRole.User => "Пользователи системы",
                _ => "Группа не назначена"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}