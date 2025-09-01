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
            if (value is TicketStatus status)
            {
                return status switch
                {
                    TicketStatus.Active => Brushes.Green,
                    TicketStatus.InProgress => Brushes.Orange,
                    TicketStatus.Closed => Brushes.MediumVioletRed,
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
