using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.AuthService;
using CRMS.Business.Services.TicketService;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CRMS.ViewModels.Support
{
    public partial class SupportTicketsViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private Ticket _selectedMyTicket;
        private Ticket _selectedActiveTicket;

        public ObservableCollection<Ticket> MyTickets { get; } = new();
        public ObservableCollection<Ticket> AllActiveTickets { get; } = new();

        public Ticket SelectedMyTicket
        {
            get => _selectedMyTicket;
            set => SetProperty(ref _selectedMyTicket, value);
        }

        public Ticket SelectedActiveTicket
        {
            get => _selectedActiveTicket;
            set => SetProperty(ref _selectedActiveTicket, value);
        }

        public SupportTicketsViewModel(ITicketService ticketService, IAuthService authService,
            IUserService userService)
        {
            _ticketService = ticketService;
            _authService = authService;
            _userService = userService;
            LoadTickets();
        }

        private async void LoadTickets()
        {
            var currentUser = _authService.CurrentUser;

            // Для "Мои тикеты"
            var myTickets = await _ticketService.GetTicketsByAssignee(currentUser.Id);
            MyTickets.Clear();
            foreach (var ticket in myTickets)
            {
                var requestor = await _userService.GetUserByIdAsync(ticket.RequestorId);
                ticket.Requestor = requestor;
                MyTickets.Add(ticket);
            }

            // Для "Все активные тикеты"
            var activeTickets = await _ticketService.GetAllActiveTickets();
            AllActiveTickets.Clear();
            foreach (var ticket in activeTickets)
            {
                var requestor = await _userService.GetUserByIdAsync(ticket.RequestorId);
                ticket.Requestor = requestor;
                AllActiveTickets.Add(ticket);
            }
        }

        [RelayCommand]
        private async Task TakeTicket(Ticket ticket)
        {
            if (ticket == null) return;

            await _ticketService.AssignTicket(ticket, _authService.CurrentUser.Id);
            LoadTickets();
        }
        [RelayCommand]
        private async Task CloseTicket(Ticket ticket)
        {
            if (ticket == null) return;
            await _ticketService.CloseTicket(ticket);
            LoadTickets();
        }
        [RelayCommand]
        private async Task CancelTicket(Ticket ticket)
        {
            if (ticket == null) return;
            await _ticketService.UnassignTicket(ticket);
            LoadTickets();

        }
        [RelayCommand]
        private async Task Refresh()
        {
            LoadTickets();
        }

    }   
}
