using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class FaqVote
    {
        public int Id { get; set; }

        public int FaqItemId { get; set; }
        public virtual FaqItem FaqItem { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public bool IsPositive { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }

}
