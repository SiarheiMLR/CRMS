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
        public User Creator { get; set; }
        public string Type { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
