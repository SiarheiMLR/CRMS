using CRMS.Domain.Entities;
using CRMS.ViewModels;
using MahApps.Metro.Controls;

namespace CRMS.Views.User.TicketEdit
{
    public partial class TicketEditWindow : MetroWindow
    {
        public TicketEditWindow(Ticket ticket = null)
        {
            InitializeComponent();
            DataContext = new TicketEditViewModel(ticket);
        }

        public Ticket GetEditedTicket()
        {
            return (DataContext as TicketEditViewModel)?.GetTicket();
        }
    }
}
