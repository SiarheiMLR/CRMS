using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using System.Linq.Expressions;

namespace CRMS.Business.Services.TicketService
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TicketService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task AddTicketAsync(Ticket ticket)
        {
            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AssignTicket(Ticket ticket, int supportId)
        {
            ticket.SupporterId = supportId;
            ticket.Status = TicketStatus.InProgress;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CloseTicket(Ticket ticket)
        {
            ticket.Status = TicketStatus.Closed;
            await _unitOfWork.SaveChangesAsync();
        }

        public void DeleteTicket(Ticket ticket)
        {
            _unitOfWork.Tickets.Remove(ticket);
            _unitOfWork.SaveChanges();
        }

        public async Task<IEnumerable<Ticket>> FindTicketsAsync(Expression<Func<Ticket, bool>> predicate)
        {
            return await _unitOfWork.Tickets.GetWhereAsync(predicate);
        }

        public async Task<IEnumerable<Ticket>> FindTicketsNoTrackingAsync(Expression<Func<Ticket, bool>> predicate)
        {
            return await _unitOfWork.Tickets.GetWhereNoTrackingAsync(predicate);
        }

        public async Task<IEnumerable<Ticket>> GetAllActiveTickets()
        {
            return await _unitOfWork.Tickets.GetWhereAsync(t => t.Status == TicketStatus.Active);
        }

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await _unitOfWork.Tickets.GetAllAsync();
        }

        public async Task<Ticket> GetTicketByIdAsync(int id)
        {
            return await _unitOfWork.Tickets.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByAssignee(int supportId)
        {
            return await _unitOfWork.Tickets.GetWhereAsync(t => t.SupporterId == supportId &&
            t.Status == TicketStatus.InProgress);
        }

        public async Task UnassignTicket(Ticket ticket)
        {
            ticket.Status = TicketStatus.Active;
            ticket.SupporterId = null;
            await _unitOfWork.SaveChangesAsync();
        }

        public async void UpdateTicket(Ticket ticket)
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

