using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class CustomFieldValue
    {
        public int Id { get; set; }
        public int CustomFieldId { get; set; }
        public CustomField CustomField { get; set; }
        public string Value { get; set; }
    }
}
