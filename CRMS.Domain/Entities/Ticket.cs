using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Domain.Entities
{
    public enum TicketStatus
    {
        
        Active,
        InProgress,
        Closed
    }
    public enum TicketPriority
    {
        High,
        Mid,
        Low
    }
    public class Ticket
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }

        public int RequestorId { get; set; }
        public User Requestor { get; set; }

        public int? SupporterId { get; set; }
        public User? Supporter { get; set; }

        public int? QueueId { get; set; }
        public Queue Queue { get; set; }

        [NotMapped]
        public string TicketNumber => $"#{Id:D3}";

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
    }
}
