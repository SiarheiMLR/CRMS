using CRMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class FaqItem
    {
        public int Id { get; set; }        

        [Required]
        public string Question { get; set; } = string.Empty;

        [Required]
        public string AnswerMarkdown { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public virtual FaqCategory Category { get; set; } = null!;

        public int AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;

        public bool IsPublished { get; set; } = false;

        public FaqStatus Status { get; set; } = FaqStatus.Draft;

        public int Views { get; set; } = 0;
        public int PositiveVotes { get; set; } = 0;
        public int NegativeVotes { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<FaqVote> Votes { get; set; } = new List<FaqVote>();

        public ICollection<FaqItemTag> FaqItemTags { get; set; } = new List<FaqItemTag>();

        public ICollection<FaqItemHistory> History { get; set; } = new List<FaqItemHistory>();
    }

}
