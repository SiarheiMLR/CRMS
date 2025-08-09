using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class FaqTag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<FaqItemTag> FaqItemTags { get; set; } = new List<FaqItemTag>();
    }

}
