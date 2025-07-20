using System;
using System.Globalization;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class NullToTextConverter : IValueConverter
    {
        public string NullText { get; set; } = "Не указано";
        public string DateTimeFormat { get; set; } = "dd.MM.yyyy HH:mm:ss";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если значение null, возвращаем текст по умолчанию или из параметра
            if (value == null)
                return parameter as string ?? NullText;

            // Если значение - DateTime или DateTime?
            if (value is DateTime dateTime)
            {
                // Преобразуем UTC в локальное время
                if (dateTime.Kind == DateTimeKind.Utc)
                    dateTime = dateTime.ToLocalTime();

                return dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
            }

            // Для всех остальных типов возвращаем значение как есть
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
