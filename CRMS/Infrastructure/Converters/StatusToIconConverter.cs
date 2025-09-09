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
    public class StatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketStatus status)
            {
                switch (status)
                {
                    case TicketStatus.Active: return FontAwesomeIcon.PlayCircle;
                    case TicketStatus.InProgress: return FontAwesomeIcon.Cog;                    
                    case TicketStatus.Closed: return FontAwesomeIcon.CheckCircle;                    
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
