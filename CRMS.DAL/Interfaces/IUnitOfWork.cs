using CRMS.DAL.Repositories;

namespace CRMS.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        // Свойства возвращают репозитории сущностей
        public EfAttachmentsRepository Attachments { get; }
        public EfTicketsRepository Tickets { get; }
        public EfUsersRepository Users { get; }
        //IRepository<CustomField> CustomFieldsRepository { get; }
        //IRepository<CustomFieldValue> CustomFieldValuesRepository { get; }
        //IRepository<Group> GroupsRepository { get; }
        //IRepository<GroupMember> GroupMembersRepository { get; }
        //IRepository<Transaction> TransactionsRepository { get; }

        // Методы сохраняют все изменения в БД
        Task<int> SaveChangesAsync();
        void SaveChanges();
    }
}
