using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class FaqItemHistory
    {
        public int Id { get; set; }

        public int FaqItemId { get; set; }
        public FaqItem FaqItem { get; set; }

        public string AnswerMarkdown { get; set; } = string.Empty;

        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
        public string EditedBy { get; set; } = "Неизвестно";
    }

}
