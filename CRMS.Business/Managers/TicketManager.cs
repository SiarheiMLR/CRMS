using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Managers
{
    public class TicketManager : BaseManager
    {
        public TicketManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // Получение всех тикетов
        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync() => await ticketRepository.GetAllAsync();

        // Получение тикета по Id
        public async Task<Ticket> GetTicketByIdAsync(int id) => await ticketRepository.GetByIdAsync(id);

        // Добавление нового тикета
        public async Task AddTicketAsync(Ticket ticket)
        {
            await ticketRepository.AddAsync(ticket);
            await unitOfWork.SaveChangesAsync(); // Сохраняем изменения
        }

        // Обновление тикета
        public void UpdateTicket(Ticket ticket)
        {
            ticketRepository.Update(ticket);
            unitOfWork.SaveChanges(); // Сохраняем изменения
        }

        // Удаление тикета
        public void DeleteTicket(Ticket ticket)
        {
            ticketRepository.Remove(ticket);
            unitOfWork.SaveChanges(); // Сохраняем изменения
        }

        // Поиск тикетов по предикату
        public async Task<IEnumerable<Ticket>> FindTicketsAsync(Expression<Func<Ticket, bool>> predicate)
            => await ticketRepository.FindAsync(predicate);
    }
}
