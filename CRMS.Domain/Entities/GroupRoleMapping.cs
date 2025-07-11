using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Domain.Entities
{
    public class GroupRoleMapping
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Group")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        public UserRole Role { get; set; }
    }
}
