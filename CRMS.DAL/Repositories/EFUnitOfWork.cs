using CRMS.DAL.Data;
using CRMS.DAL.Repositories;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.DAL.Repositories
{
    public class EFUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly CRMSDbContext _context;

        public EFUnitOfWork(CRMSDbContext context)
        {
            _context = context;
            Attachments = new EfAttachmentsRepository(_context);
            Tickets = new EfTicketsRepository(_context);
            Queues = new EfQueuesRepository(_context);
            Users = new EfUsersRepository(_context);
            GroupsRepository = new EfGroupsRepository(_context);
            GroupMembersRepository = new EfGroupMembersRepository(_context);
            GroupRoleMappings = new Repository<GroupRoleMapping>(context);

            // Модель FAQ
            FaqItemsRepository = new EfFaqItemsRepository(_context);
            FaqCategoriesRepository = new EfFaqCategoriesRepository(_context);
            FaqTagsRepository = new EfFaqTagsRepository(_context);
            FaqVotesRepository = new EfFaqVotesRepository(_context);
            FaqItemTagsRepository = new EfFaqItemTagsRepository(_context);
            FaqItemHistoriesRepository = new EfFaqItemHistoriesRepository(_context);
        }

        public EfAttachmentsRepository Attachments { get; }
        public EfTicketsRepository Tickets { get; }
        public EfQueuesRepository Queues { get; }
        public EfUsersRepository Users { get; }

        // Модель FAQ
        public EfFaqItemsRepository FaqItemsRepository { get; }
        public EfFaqCategoriesRepository FaqCategoriesRepository { get; }
        public EfFaqTagsRepository FaqTagsRepository { get; }
        public EfFaqVotesRepository FaqVotesRepository { get; }
        public EfFaqItemTagsRepository FaqItemTagsRepository { get; }
        public EfFaqItemHistoriesRepository FaqItemHistoriesRepository { get; }

        public IRepository<Group> GroupsRepository { get; }
        public IRepository<GroupMember> GroupMembersRepository { get; }
        public IRepository<GroupRoleMapping> GroupRoleMappings { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
