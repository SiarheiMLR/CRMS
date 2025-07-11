using CRMS.Domain.Entities;
using System.Collections.Generic;

namespace CRMS.Business.Model
{
    public class GroupWithMembers
    {
        public Group Group { get; set; } = default!;
        public List<User> Members { get; set; } = new();        

        // Удобные прокси-свойства
        public int Id => Group.Id;
        public string Name => Group.Name;
        public string Description => Group.Description;
        public bool IsSystemGroup => Group.IsSystemGroup;
    }
}
