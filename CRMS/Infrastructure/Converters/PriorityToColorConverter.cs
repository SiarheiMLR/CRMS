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
                    _ => Brushes.Gray // Используйте нейтральный цвет вместо черного
                };
            }

            // Возвращаем значение по умолчанию для некорректных данных
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
