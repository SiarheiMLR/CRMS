using System;
using System.Globalization;
using System.Windows.Data;
using CRMS.Domain.Entities;

namespace CRMS.Infrastructure.Converters
{
    public class StatusToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketStatus status && parameter is string expectedStatus)
            {
                return status.ToString() == expectedStatus;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
