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
        public void UpdateTicket(Ticket ticket);
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
    }
}