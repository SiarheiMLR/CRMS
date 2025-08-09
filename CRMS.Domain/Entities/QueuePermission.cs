using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Domain.Entities
{
    public enum QueuePermissionType
    {
        View,
        Create,
        Comment,
        Assign
    }

    public class QueuePermission
    {
        public int Id { get; set; }

        public int QueueId { get; set; }
        public Queue Queue { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public QueuePermissionType Permission { get; set; }
    }
}
