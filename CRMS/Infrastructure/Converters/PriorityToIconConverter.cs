using CRMS.Domain.Entities;
using FontAwesome.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CRMS.Infrastructure.Converters
{
    public class PriorityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketPriority priority)
            {
                switch (priority)
                {
                    case TicketPriority.Low: return FontAwesomeIcon.ArrowDown;
                    case TicketPriority.Mid: return FontAwesomeIcon.Minus;
                    case TicketPriority.High: return FontAwesomeIcon.ArrowUp;                    
                    default: return FontAwesomeIcon.QuestionCircle;
                }
            }
            return FontAwesomeIcon.QuestionCircle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
