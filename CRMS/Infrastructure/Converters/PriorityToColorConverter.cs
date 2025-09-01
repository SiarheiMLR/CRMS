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
            if (value is TicketPriority priority)
            {
                return priority switch
                {
                    TicketPriority.High => Brushes.Red,
                    TicketPriority.Mid => Brushes.Orange,
                    TicketPriority.Low => Brushes.Green,
                    _ => Brushes.White // Белый для "Все"
                };
            }

            return Brushes.White; // Белый для null значений
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
