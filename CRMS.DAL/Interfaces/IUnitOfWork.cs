using CRMS.DAL.Repositories;
using CRMS.Domain.Entities;

namespace CRMS.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        // Свойства возвращают репозитории сущностей
        public EfAttachmentsRepository Attachments { get; }
        public EfTicketsRepository Tickets { get; }
        public EfQueuesRepository Queues { get; }
        public EfUsersRepository Users { get; }
        public EfTransactionsRepository Transactions { get; } // Добавлено свойство для Transactions
        public EfTicketCommentsRepository TicketComments { get; } // Добавлено свойство для TicketComments
        //IRepository<CustomField> CustomFieldsRepository { get; }
        //IRepository<CustomFieldValue> CustomFieldValuesRepository { get; }

        // Модель FAQ
        public EfFaqItemsRepository FaqItemsRepository { get; }
        public EfFaqCategoriesRepository FaqCategoriesRepository { get; }
        public EfFaqTagsRepository FaqTagsRepository { get; }
        public EfFaqVotesRepository FaqVotesRepository { get; }
        public EfFaqItemTagsRepository FaqItemTagsRepository { get; }
        public EfFaqItemHistoriesRepository FaqItemHistoriesRepository { get; }

        IRepository<Group> GroupsRepository { get; }
        IRepository<GroupMember> GroupMembersRepository { get; }
        IRepository<GroupRoleMapping> GroupRoleMappings { get; }        

        // Методы сохраняют все изменения в БД
        Task<int> SaveChangesAsync();
        void SaveChanges();
    }
}
