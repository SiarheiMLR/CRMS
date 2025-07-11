using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;

namespace CRMS.Infrastruсture.Converters
{
    public class RoleFromGroupsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is User user && user.GroupMembers != null)
            {
                var role = RoleMapper.ResolveRole(user);

                return role switch
                {
                    UserRole.Admin => "Администраторы системы",
                    UserRole.Support => "Служба поддержки",
                    UserRole.User => "Пользователи системы",
                    _ => "Не указано"
                };
            }

            return "Не указано";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

