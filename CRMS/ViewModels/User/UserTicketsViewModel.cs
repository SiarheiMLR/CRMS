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

namespace CRMS.ViewModels.UserVM
{
    public partial class UserTicketsViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public ObservableCollection<Ticket> Tickets { get; } = new();

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
            var tickets = await _ticketService.FindTicketsNoTrackingAsync(t => t.RequestorId == CurrentUser.Id);

            foreach (var ticket in tickets)
            {
                if (ticket.Status is TicketStatus.InProgress or TicketStatus.Closed)
                {
                    if (ticket.SupporterId != null)
                    {
                        var supporter = await _userService.GetUserByIdAsync(ticket.SupporterId ?? 1);
                        ticket.Supporter = supporter;
                    }
                }
            }
            Tickets.Clear();
            foreach (var ticket in tickets)
            {
                Tickets.Add(ticket);
            }
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
                Created = DateTime.Now,
                RequestorId = CurrentUser.Id,
                SupporterId = null,
            };

            var editWindow = new TicketEditWindow(newTicket);
            if (editWindow.ShowDialog() == true)
            {
                Tickets.Add(newTicket);
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