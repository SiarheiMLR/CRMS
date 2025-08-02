using CRMS.Domain.Entities;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace CRMS.Infrastructure.Converters
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketStatus status)
            {
                return status == TicketStatus.InProgress || status == TicketStatus.Closed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
