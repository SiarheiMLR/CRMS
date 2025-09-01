using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.AuthService;
using CRMS.Business.Services.QueueService;
using CRMS.Business.Services.TicketService;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Windows.Documents;                                         // PageContent, FixedPage, FixedDocument...
using System.IO;
using System.Text;
using CRMS.Views.User;
using static System.Net.Mime.ContentType;
using System.Globalization;
using Microsoft.Win32;
using System.Windows.Xps.Packaging;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Windows.Media;                                             // Visual, PixelFormats
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Input;
using CRMS.Business.Services.DocumentService;
using CRMS.Business.Services.EmailService;
using CRMS.Business.Services.EmailService.Templates;
using CRMS.Helpers;
using System.Diagnostics;

namespace CRMS.ViewModels.Support
{
    public partial class SupportTicketsViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;
        private readonly IDocumentConverter _documentConverter;
        private readonly IEmailService _emailService;
        private readonly IMessenger _messenger;

        public ICommand ShowFullContentCommand { get; }

        // Правильный формат даты и времени
        private string _currentDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        public string CurrentDate
        {
            get => _currentDate;
            set => SetProperty(ref _currentDate, value);
        }

        private string _currentTime = DateTime.Now.ToString("HH:mm:ss");
        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        // ==== Коллекции для фильтров ====
        public ObservableCollection<FilterOption<TicketStatus>> Statuses { get; } = new();
        public ObservableCollection<FilterOption<TicketPriority>> Priorities { get; } = new();
        public ObservableCollection<FilterOptionRef<Queue>> Queues { get; } = new();
        public ObservableCollection<FilterOptionRef<User>> Creators { get; } = new();

        // ==== Выбранные значения фильтров ====
        [ObservableProperty] private FilterOption<TicketStatus> _selectedStatus;
        [ObservableProperty] private FilterOption<TicketPriority> _selectedPriority;
        [ObservableProperty] private FilterOptionRef<Queue> _selectedQueue;
        [ObservableProperty] private FilterOptionRef<User> _selectedCreator;

        // Конструктор класса
        public SupportTicketsViewModel(ITicketService ticketService, IAuthService authService,
            IUserService userService, IQueueService queueService, IEmailService emailService,
            IDocumentConverter documentConverter, IMessenger messenger = null)
        {
            // Проверка на режим дизайна
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _ticketService = ticketService;
            _auth_service_check(authService); // helper below
            _authService = authService;
            _userService = userService;
            _queueService = queueService;
            _documentConverter = documentConverter;
            _emailService = emailService;

            //CurrentUser = _authService.CurrentUser;

            LoadTickets();

            // сразу подгружаем списки для комбобоксов
            _ = InitializeFiltersAsync();
        }

        private async Task InitializeFiltersAsync()
        {
            try
            {
                // Статусы
                Statuses.Add(new FilterOption<TicketStatus>(null, "All statuses"));
                foreach (var status in Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>())
                {
                    Statuses.Add(new FilterOption<TicketStatus>(status, status.ToString()));
                }

                // Приоритеты
                Priorities.Add(new FilterOption<TicketPriority>(null, "All priorities"));
                foreach (var priority in Enum.GetValues(typeof(TicketPriority)).Cast<TicketPriority>())
                {
                    Priorities.Add(new FilterOption<TicketPriority>(priority, priority.ToString()));
                }

                // Очереди (только с id 1 и 2)
                Queues.Add(new FilterOptionRef<Queue>(null, "Все типы запросов"));
                var queues = await _queueService.GetAllAsync();
                foreach (var queue in queues.Where(q => q.Id == 1 || q.Id == 2))
                {
                    Queues.Add(new FilterOptionRef<Queue>(queue, queue.Name));
                }

                // Авторы
                Creators.Add(new FilterOptionRef<User>(null, "Все пользователи"));
                var tickets = await _ticketService.GetAllTicketsAsync();
                var creatorIds = tickets.Select(t => t.RequestorId).Distinct().ToList();

                foreach (var creatorId in creatorIds)
                {
                    var user = await _userService.GetUserByIdAsync(creatorId);
                    if (user != null && !Creators.Any(c => c.Value != null && c.Value.Id == user.Id))
                    {
                        Creators.Add(new FilterOptionRef<User>(user, user.DisplayName));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки фильтров: {ex.Message}");
            }
        }


        /// <summary>
        /// Старый ViewModel
        /// </summary>

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

        // Вспомогательный проверочный метод (чтобы не забыть _authService в коде)
        private void _auth_service_check(IAuthService auth) { /* no-op, просто для соблюдения порядка параметров */ }

    }   
}
