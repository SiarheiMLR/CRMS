using CRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Helpers
{
    public class CommentAddedMessage
    {
        public int TicketId { get; set; }
        public TicketComment Comment { get; set; }
    }
}
