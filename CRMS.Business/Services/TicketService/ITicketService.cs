using CRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace CRMS.Business.Services.TicketService
{
    public interface ITicketService
    {
        public Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        public Task<Ticket> GetTicketByIdAsync(int id);
        public Task AddTicketAsync(Ticket ticket);
        Task UpdateTicketAsync(Ticket ticket);
        public void DeleteTicket(Ticket ticket);
        public Task<IEnumerable<Ticket>> FindTicketsAsync(Expression<Func<Ticket, bool>> predicate);
        Task<IEnumerable<Ticket>> FindTicketsNoTrackingAsync(Expression<Func<Ticket, bool>> predicate);
        public Task<IEnumerable<Ticket>> GetTicketsByAssignee(int supportId);
        public Task<IEnumerable<Ticket>> GetAllActiveTickets();
        public Task AssignTicket(Ticket ticket, int supportId);
        public Task CloseTicket(Ticket ticket);
        public Task UnassignTicket(Ticket ticket);
        Task<IEnumerable<Ticket>> FindTicketsWithDetailsAsync(Expression<Func<Ticket, bool>> predicate, Func<IQueryable<Ticket>, IIncludableQueryable<Ticket, object>> include = null);
        public Task MigrateTicketContentFormat();
        Task<IEnumerable<Ticket>> GetAllTicketsWithDetailsAsync();

        // Новые методы для работы с транзакциями
        Task AddTransactionAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetTransactionsAsync(int? userId = null, string actionType = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Transaction>> GetUserUnassignStats(int userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetUserReopenStats(int userId, DateTime startDate, DateTime endDate);

        // Новые методы для работы с комментариями
        Task<TicketComment> AddCommentAsync(TicketComment comment);
        Task<TicketComment> AddCommentAsync(int ticketId, int userId, string content, bool isInternal = false);
        Task<IEnumerable<TicketComment>> GetCommentsByTicketIdAsync(int ticketId);
    }
}