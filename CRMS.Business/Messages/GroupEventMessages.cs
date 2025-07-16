using CRMS.Domain.Entities;

namespace CRMS.Business.Messages
{
    public class GroupCreatedMessage
    {
        public Group Group { get; }

        public GroupCreatedMessage(Group group) => Group = group;
    }

    public class GroupUpdatedMessage
    {
        public Group Group { get; }

        public GroupUpdatedMessage(Group group) => Group = group;
    }

    public class GroupDeletedMessage
    {
        public int GroupId { get; }

        public GroupDeletedMessage(int groupId) => GroupId = groupId;
    }
}

