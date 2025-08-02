using CRMS.Domain.Entities;
using System.Globalization;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class StatusToRussianConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketStatus status)
            {
                return status switch
                {
                    TicketStatus.Active => "Активный",
                    TicketStatus.InProgress => "В работе",
                    TicketStatus.Closed => "Закрыт",
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
