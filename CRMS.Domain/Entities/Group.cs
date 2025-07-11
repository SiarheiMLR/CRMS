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
        public string Description { get; set; } = string.Empty;
        public bool IsSystemGroup { get; set; }
        public GroupRoleMapping? GroupRoleMapping { get; set; }

        // Явное отношение "многие ко многим" через GroupMember
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    }
}
