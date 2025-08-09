using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CRMS.Domain.Entities
{
    public class FaqCategory
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public virtual ICollection<FaqItem> Items { get; set; } = new List<FaqItem>();
    }

}
