using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.DTOs
{
    public class CategoryStatDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int FaqCount { get; set; }
    }
}
