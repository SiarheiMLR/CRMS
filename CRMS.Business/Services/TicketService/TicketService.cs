using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Windows.Documents;
using CRMS.Business.Services.DocumentService;

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
            const int maxSingleFileSize = 30 * 1024 * 1024; // 30MB
            const int maxTotalSize = 150 * 1024 * 1024; // 150MB

            // Проверка размеров файлов
            foreach (var attachment in ticket.Attachments)
            {
                if (attachment.FileData.Length > maxSingleFileSize)
                    throw new Exception($"Файл {attachment.FileName} превышает лимит 30MB");
            }

            if (ticket.Attachments.Sum(a => a.FileData.Length) > maxTotalSize)
                throw new Exception("Общий размер вложений превышает 150MB");

            // Конвертируем перед сохранением
            ticket.Content = ticket.Content;

            // Добавляем основной тикет
            await _unitOfWork.Tickets.AddAsync(ticket);

            // Добавляем вложения
            foreach (var attachment in ticket.Attachments)
            {
                attachment.TicketId = ticket.Id; // Устанавливаем связь
                await _unitOfWork.Attachments.AddAsync(attachment);
            }

            // Добавляем кастомные поля
            //foreach (var field in ticket.CustomFieldValues)
            //{
            //    field.TicketId = ticket.Id;
            //    await _unitOfWork.CustomFieldValues.AddAsync(field);
            //}

            //try
            //{
            //    using var transaction = await _unitOfWork.BeginTransactionAsync();

            //    // ... код добавления ...

            //    await _unitOfWork.SaveChangesAsync();
            //    await transaction.CommitAsync();
            //}
            //catch (Exception ex)
            //{
            //    // Откат транзакции и логирование
            //    _logger.LogError(ex, "Ошибка при создании тикета");
            //    throw;
            //}

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

        public async Task<IEnumerable<Ticket>> FindTicketsWithDetailsAsync(
             Expression<Func<Ticket, bool>> predicate,
             Func<IQueryable<Ticket>, IIncludableQueryable<Ticket, object>> include = null)
        {
            return await _unitOfWork.Tickets.FindWithIncludesAsync(predicate, include);
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
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
            if (ticket != null)
            {
                // Автоматически конвертируем при загрузке
                var _ = ticket.ContentDocument;
            }
            return ticket;
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

        public async Task MigrateTicketContentFormat()
        {
            var tickets = await _unitOfWork.Tickets.GetAllAsync();

            foreach (var ticket in tickets)
            {
                // Если содержимое не является валидным XAML
                if (!string.IsNullOrWhiteSpace(ticket.Content) &&
            !ticket.Content.TrimStart().StartsWith("<"))
                {
                    // Создаем FlowDocument из старого текста
                    var document = new FlowDocument();
                    document.Blocks.Add(new Paragraph(new Run(ticket.Content)));

                    // Используем статический метод через класс Ticket
                    ticket.Content = Ticket.ConvertFlowDocumentToXaml(document);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

    }
}