using CRMS.Domain.Entities;
using System.Globalization;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class PriorityToRussianConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketPriority priority)
            {
                return priority switch
                {
                    TicketPriority.High => "Высокий",
                    TicketPriority.Mid => "Средний",
                    TicketPriority.Low => "Низкий",
                    _ => value.ToString()
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
