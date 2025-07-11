using CRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.ViewModels.Admin.Groups
{
    public class RoleOption
    {
        public UserRole Role { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
