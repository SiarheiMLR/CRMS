using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class Queue
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CorrespondAddress { get; set; } = string.Empty;
        public string CommentAddress { get; set; } = string.Empty;

        public TimeSpan? SlaResponseTime { get; set; }
        public TimeSpan? SlaResolutionTime { get; set; }

        public int? ParentQueueId { get; set; }
        public Queue? ParentQueue { get; set; }
        public ICollection<Queue> SubQueues { get; set; } = new List<Queue>();

        public ICollection<TicketStatusDefinition> Statuses { get; set; } = new List<TicketStatusDefinition>();
        public ICollection<QueuePermission> Permissions { get; set; } = new List<QueuePermission>();
        public ICollection<QueueAutomationRule> AutomationRules { get; set; } = new List<QueueAutomationRule>();

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
