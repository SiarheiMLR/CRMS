using CRMS.Domain.Entities;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (TicketPriority)value switch
            {
                TicketPriority.High => Brushes.Red,
                TicketPriority.Mid => Brushes.Orange,
                TicketPriority.Low => Brushes.Green,
                _ => Brushes.Black
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
