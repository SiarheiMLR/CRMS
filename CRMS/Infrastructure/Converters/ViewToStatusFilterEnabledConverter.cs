using System;
using System.Globalization;
using System.Windows.Data;
using static CRMS.ViewModels.Support.SupportTicketsViewModel;

namespace CRMS.Infrastructure.Converters
{
    public class ViewToStatusFilterEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ViewType viewType)
            {
                // Разрешаем фильтр по статусу только вне фиксированных вкладок
                return viewType != ViewType.MyTickets &&
                       viewType != ViewType.NewTickets &&
                       viewType != ViewType.ClosedTickets;
            }

            return true; // по умолчанию включён
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
