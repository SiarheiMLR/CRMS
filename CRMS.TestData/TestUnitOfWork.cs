using System.Collections.Generic;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.TestData
{
    public class TestUnitOfWork : IUnitOfWork
    {
        private IRepository<Attachment> attachmentsRepository;
        private IRepository<CustomField> customFieldsRepository;
        private IRepository<CustomFieldValue> customFieldValuesRepository;
        private IRepository<Group> groupsRepository;
        private IRepository<GroupMember> groupMembersRepository;
        private IRepository<Queue> queuesRepository;
        private IRepository<Ticket> ticketsRepository;
        private IRepository<Transaction> transactionsRepository;
        private IRepository<User> usersRepository;

        private List<Attachment> attachments;
        private List<CustomField> customFields;
        private List<CustomFieldValue> customFieldValues;
        private List<Group> groups;
        private List<GroupMember> groupMembers;
        private List<Queue> queues;
        private List<Ticket> tickets;
        private List<Transaction> transactions;
        private List<User> users;

        public TestUnitOfWork()
        {
            attachments = new List<Attachment>();
            customFields = new List<CustomField>();
            customFieldValues = new List<CustomFieldValue>();
            groups = new List<Group>();
            groupMembers = new List<GroupMember>();
            queues = new List<Queue>();
            tickets = new List<Ticket>();
            transactions = new List<Transaction>();
            users = new List<User>();

            attachmentsRepository = new TestRepository<Attachment>(attachments);
            customFieldsRepository = new TestRepository<CustomField>(customFields);
            customFieldValuesRepository = new TestRepository<CustomFieldValue>(customFieldValues);
            groupsRepository = new TestRepository<Group>(groups);
            groupMembersRepository = new TestRepository<GroupMember>(groupMembers);
            queuesRepository = new TestRepository<Queue>(queues);
            ticketsRepository = new TestRepository<Ticket>(tickets);
            transactionsRepository = new TestRepository<Transaction>(transactions);
            usersRepository = new TestRepository<User>(users);
        }

        public IRepository<Attachment> AttachmentsRepository => attachmentsRepository;
        public IRepository<CustomField> CustomFieldsRepository => customFieldsRepository;
        public IRepository<CustomFieldValue> CustomFieldValuesRepository => customFieldValuesRepository;
        public IRepository<Group> GroupsRepository => groupsRepository;
        public IRepository<GroupMember> GroupMembersRepository => groupMembersRepository;
        public IRepository<Queue> QueuesRepository => queuesRepository;
        public IRepository<Ticket> TicketsRepository => ticketsRepository;
        public IRepository<Transaction> TransactionsRepository => transactionsRepository;
        public IRepository<User> UsersRepository => usersRepository;        
           
        Task<int> IUnitOfWork.SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
        public void SaveChanges()
        {
        }
    }
}