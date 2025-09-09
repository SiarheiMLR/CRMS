using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public string ActionType { get; set; } // "Take", "Unassign", "Reopen", "Close"
        public string Details { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
