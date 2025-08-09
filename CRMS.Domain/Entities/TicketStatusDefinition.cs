using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class TicketStatusDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int QueueId { get; set; }
        public Queue Queue { get; set; }
    }
}
