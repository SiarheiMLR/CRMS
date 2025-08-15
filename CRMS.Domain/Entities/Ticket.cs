using System.ComponentModel;
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
    public class Ticket
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty; // Храним XAML
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

        [NotMapped] // Не сохраняем в БД
        public string TicketNumber => $"#{Id:D3}";

        [NotMapped] // Не сохраняем в БД
        public FlowDocument ContentDocument
        {
            get
            {
                try
                {
                    return ConvertXamlToFlowDocument(Content);
                }
                catch
                {
                    return new FlowDocument();
                }
            }
            set
            {
                try
                {
                    Content = ConvertFlowDocumentToXaml(value);
                }
                catch
                {
                    Content = string.Empty;
                }
            }
        }

        public static string ConvertFlowDocumentToXaml(FlowDocument document)
        {
            if (document == null || document.Blocks.Count == 0)
                return string.Empty;

            try
            {
                var range = new TextRange(document.ContentStart, document.ContentEnd);
                using (var stream = new MemoryStream())
                {
                    range.Save(stream, DataFormats.Xaml);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch
            {
                // Если не удалось сохранить как XAML, сохраняем как текст
                return new TextRange(document.ContentStart, document.ContentEnd).Text;
            }
        }

        public static FlowDocument ConvertXamlToFlowDocument(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new FlowDocument();

            // Проверяем, является ли содержимое валидным XAML
            if (IsValidXaml(content))
            {
                try
                {
                    var document = new FlowDocument();
                    var range = new TextRange(document.ContentStart, document.ContentEnd);
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        range.Load(stream, DataFormats.Xaml);
                    }
                    return document;
                }
                catch
                {
                    // Если не удалось загрузить как XAML, возвращаем как текст
                    return CreateTextFlowDocument(content);
                }
            }

            // Если не XAML, обрабатываем как обычный текст
            return CreateTextFlowDocument(content);
        }

        private static bool IsValidXaml(string content)
        {
            return !string.IsNullOrWhiteSpace(content) &&
                   content.TrimStart().StartsWith("<");
        }

        private static FlowDocument CreateTextFlowDocument(string text)
        {
            var document = new FlowDocument();
            document.Blocks.Add(new Paragraph(new Run(text)));
            return document;
        }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
    }
}
