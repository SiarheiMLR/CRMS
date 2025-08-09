using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class QueueAutomationRule
    {
        public int Id { get; set; }

        public int QueueId { get; set; }
        public Queue Queue { get; set; }

        public string TriggerEvent { get; set; } = string.Empty; // e.g. "TicketCreated"
        public string Condition { get; set; } = string.Empty;    // e.g. "Priority == 'High'"
        public string Action { get; set; } = string.Empty;       // e.g. "SendEmail"
    }
}
