using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public bool IsSystemGroup { get; set; }
        
        // Связи "многие ко многим"
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    }
}
