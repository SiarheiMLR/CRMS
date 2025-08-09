using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.AuthService;
using CRMS.Business.Services.TicketService;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Views.User.TicketEdit;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMS.ViewModels.UserVM
{
    public partial class UserTicketsViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public ObservableCollection<Ticket> Tickets { get; } = new();
        public ObservableCollection<Ticket> OpenTickets { get; } = new();
        public ObservableCollection<Ticket> ClosedTickets { get; } = new();

        [ObservableProperty]
        private ICollectionView _groupedTickets;
        [ObservableProperty]
        private User _currentUser;

        public UserTicketsViewModel(ITicketService ticketService, IAuthService authService,
            IUserService userService)
        {
            _ticketService = ticketService;
            _authService = authService;
            _userService = userService;
            CurrentUser = _authService.CurrentUser;
            LoadTickets();
        }

        private async void LoadTickets()
        {
            // var tickets = await _ticketService.FindTicketsNoTrackingAsync(t => t.RequestorId == CurrentUser.Id);
            // Загружаем тикеты с включением связанных данных
            var tickets = await _ticketService.FindTicketsWithDetailsAsync(
                t => t.RequestorId == CurrentUser.Id,
                include: q => q
                    .Include(t => t.Requestor)
                    .Include(t => t.Supporter)
                    .Include(t => t.Queue)
            );

            // Очищаем все коллекции
            Tickets.Clear();
            OpenTickets.Clear();
            ClosedTickets.Clear();

            foreach (var ticket in tickets)
            {
                // Загрузка supporter только для нужных статусов
                if (ticket.Status is TicketStatus.InProgress or TicketStatus.Closed)
                {
                    if (ticket.SupporterId != null)
                    {
                        var supporter = await _userService.GetUserByIdAsync(ticket.SupporterId ?? 1);
                        ticket.Supporter = supporter;
                    }
                }

                // Добавляем тикет только ОДИН раз
                Tickets.Add(ticket);

                // Распределяем по коллекциям
                if (ticket.Status == TicketStatus.Closed)
                    ClosedTickets.Add(ticket);
                else
                    OpenTickets.Add(ticket); // Active и InProgress
            }

            // Настройка группировки
            GroupedTickets = CollectionViewSource.GetDefaultView(Tickets);
            GroupedTickets.GroupDescriptions.Clear();
            GroupedTickets.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Ticket.Status)));
        }

        [RelayCommand]
        private async void CreateNewTicket()
        {
            var newTicket = new Ticket
            {
                Status = TicketStatus.Active,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                RequestorId = CurrentUser.Id,
                SupporterId = null,
            };

            var editWindow = new TicketEditWindow(newTicket);
            if (editWindow.ShowDialog() == true)
            {
                Tickets.Add(newTicket);
                OpenTickets.Add(newTicket); // Добавляем в коллекцию открытых
                await _ticketService.AddTicketAsync(newTicket);
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            LoadTickets();
        }
    }
}