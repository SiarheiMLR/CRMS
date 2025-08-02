using CRMS.Domain.Entities;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (TicketStatus)value switch
            {
                TicketStatus.Active => Brushes.Green,
                TicketStatus.InProgress => Brushes.Orange,
                TicketStatus.Closed => Brushes.Gray,
                _ => Brushes.Black
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
