using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

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
    public partial class Ticket
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty; // XAML-контент с изображениями в base64
        public string Subject { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        //public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Новые поля для статистики
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }

        public int RequestorId { get; set; }
        public User Requestor { get; set; }

        public int? SupporterId { get; set; }
        public User? Supporter { get; set; }

        public int? QueueId { get; set; }
        public Queue Queue { get; set; }

        [NotMapped] // Не сохраняем в БД
        public string TicketNumber => $"#{Id:D3}";
        
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}
