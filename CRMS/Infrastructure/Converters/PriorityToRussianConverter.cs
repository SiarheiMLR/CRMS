using CRMS.Domain.Entities;
using System.Globalization;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class PriorityToRussianConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Все приоритеты";

            if (value is TicketPriority priority)
            {
                return priority switch
                {
                    TicketPriority.High => "Высокий",
                    TicketPriority.Mid => "Средний",
                    TicketPriority.Low => "Низкий",
                    _ => "Неизвестно" // Более понятное значение по умолчанию
                };
            }

            // Возвращаем понятное сообщение вместо исходного значения
            return "Ошибка типа";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
