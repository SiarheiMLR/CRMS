using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class TicketComment
    {
        public int Id { get; set; }

        // Привязка к тикету
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        // Автор комментария
        public int UserId { get; set; }
        public User User { get; set; }

        // Сам текст комментария (XAML как у тикета)
        public string Content { get; set; } = string.Empty; // В XAML могут быть большие тексты

        // Дата создания
        public DateTime Created { get; set; } = DateTime.Now;

        // Если планируешь делать «внутренние комментарии» (видят только Support/Admin)
        public bool IsInternal { get; set; } = false;
    }
}
