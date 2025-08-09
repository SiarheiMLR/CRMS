using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class FaqItemTag
    {
        public int FaqItemId { get; set; }
        public FaqItem FaqItem { get; set; }
        public int FaqTagId { get; set; }
        public FaqTag FaqTag { get; set; }
    }

}
