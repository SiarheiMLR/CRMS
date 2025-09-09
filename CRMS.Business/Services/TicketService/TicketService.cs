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

        // Обновим существующие методы для добавления транзакций
        public async Task AssignTicket(Ticket ticket, int supportId)
        {
            var existing = await _unitOfWork.Tickets.GetByIdAsync(ticket.Id);
            if (existing == null) throw new Exception("Ticket not found");

            existing.SupporterId = supportId;
            existing.Status = TicketStatus.InProgress;
            existing.StartedAt = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();

            // Добавляем транзакцию
            await AddTransactionAsync(new Transaction
            {
                TicketId = ticket.Id,
                UserId = supportId,
                ActionType = "Take",
                Details = $"Заявка взята в работу пользователем ID: {supportId}",
                Created = DateTime.Now
            });
        }

        public async Task CloseTicket(Ticket ticket)
        {
            var existing = await _unitOfWork.Tickets.GetByIdAsync(ticket.Id);
            if (existing == null) throw new Exception("Ticket not found");

            existing.Status = TicketStatus.Closed;
            existing.CompletedAt = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();

            // Добавляем транзакцию
            await AddTransactionAsync(new Transaction
            {
                TicketId = ticket.Id,
                UserId = existing.SupporterId,
                ActionType = "Close",
                Details = "Заявка закрыта",
                Created = DateTime.Now
            });
        }

        public async Task UnassignTicket(Ticket ticket)
        {
            var existing = await _unitOfWork.Tickets.GetByIdAsync(ticket.Id);
            if (existing == null) throw new Exception("Ticket not found");

            // Сохраняем ID исполнителя перед обнулением
            var supporterId = existing.SupporterId;

            existing.Status = TicketStatus.Active;
            existing.SupporterId = null;
            existing.StartedAt = null;

            await _unitOfWork.SaveChangesAsync();

            // Добавляем транзакцию
            await AddTransactionAsync(new Transaction
            {
                TicketId = ticket.Id,
                UserId = supporterId,
                ActionType = "Unassign",
                Details = "Отказ от выполнения заявки",
                Created = DateTime.Now
            });
        }

        // Обновим метод UpdateTicketAsync для поддержки транзакций при изменении статуса
        public async Task UpdateTicketAsync(Ticket updatedTicket)
        {
            var existingTicket = await _unitOfWork.Tickets.GetByIdAsync(updatedTicket.Id);
            if (existingTicket == null)
                throw new Exception($"Ticket {updatedTicket.Id} not found");

            // Запоминаем старый статус для проверки изменений
            var oldStatus = existingTicket.Status;

            // Обновляем только поля, которые могут изменяться
            existingTicket.Status = updatedTicket.Status;
            existingTicket.SupporterId = updatedTicket.SupporterId;
            existingTicket.StartedAt = updatedTicket.StartedAt;
            existingTicket.CompletedAt = updatedTicket.CompletedAt;
            existingTicket.Subject = updatedTicket.Subject;
            existingTicket.Content = updatedTicket.Content;

            await _unitOfWork.SaveChangesAsync();

            // Добавляем транзакцию если статус изменился
            if (oldStatus != updatedTicket.Status)
            {
                string actionType = updatedTicket.Status switch
                {
                    TicketStatus.Active => "Reopen",
                    TicketStatus.InProgress => "Take",
                    TicketStatus.Closed => "Close",
                    _ => "StatusChange"
                };

                await AddTransactionAsync(new Transaction
                {
                    TicketId = updatedTicket.Id,
                    UserId = updatedTicket.SupporterId,
                    ActionType = actionType,
                    Details = $"Статус изменен с {oldStatus} на {updatedTicket.Status}",
                    Created = DateTime.Now
                });
            }
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

        public async Task<IEnumerable<Ticket>> GetAllTicketsWithDetailsAsync()
        {
            return await _unitOfWork.Tickets.FindWithIncludesAsync(
                predicate: t => true, // Получаем все тикеты
                includes: q => q
                    .Include(t => t.Requestor)
                    .Include(t => t.Supporter)
                    .Include(t => t.Queue)
                    .Include(t => t.Attachments)
            );
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

        // Новые методы для работы с транзакциями
        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(int? userId = null, string actionType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.Transactions.AsQueryable() // Используем метод из репозитория
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(t => t.ActionType == actionType);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Created >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Created <= endDate.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetUserUnassignStats(int userId, DateTime startDate, DateTime endDate)
        {
            return await GetTransactionsAsync(userId, "Unassign", startDate, endDate);
        }

        public async Task<IEnumerable<Transaction>> GetUserReopenStats(int userId, DateTime startDate, DateTime endDate)
        {
            return await GetTransactionsAsync(userId, "Reopen", startDate, endDate);
        }

    }
}