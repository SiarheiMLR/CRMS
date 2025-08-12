using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.AuthService;
using CRMS.Business.Services.QueueService;
using CRMS.Business.Services.TicketService;
using CRMS.Business.Services.UserService;
using CRMS.Domain.Entities;
using CRMS.Views.User.TicketEdit;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace CRMS.ViewModels.UserVM
{
    public partial class UserTicketsViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;

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

        // Коллекции тикетов
        public ObservableCollection<Ticket> Tickets { get; } = new();
        public ObservableCollection<Ticket> OpenTickets { get; } = new();
        public ObservableCollection<Ticket> ClosedTickets { get; } = new();

        // Список доступных очередей для создания тикета
        public ObservableCollection<Queue> Queues { get; } = new();

        [ObservableProperty]
        private Queue _selectedQueue; // привязка SelectedItem ComboBox       

        [ObservableProperty]
        private ICollectionView _groupedTickets;

        [ObservableProperty]
        private User _currentUser;

        public UserTicketsViewModel(ITicketService ticketService, IAuthService authService,
            IUserService userService, IQueueService queueService)
        {
            // Проверка на режим дизайна
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _ticketService = ticketService;
            _auth_service_check(authService); // helper below
            _authService = authService;
            _userService = userService;
            _queueService = queueService;

            CurrentUser = _authService.CurrentUser;

            // Запускаем загрузку асинхронно
            _ = InitializeAsync();
        }

        // Вынесем инициализацию в отдельный метод
        private async Task InitializeAsync()
        {
            await LoadQueuesForCurrentUserAsync();
            await LoadTicketsAsync();
        }

        // Загружаем очереди доступные пользователю
        private async Task LoadQueuesForCurrentUserAsync()
        {
            if (CurrentUser == null) return;

            try
            {
                var queues = await _queueService.GetQueuesForUserAsync(CurrentUser.Id);

                Queues.Clear();
                foreach (var q in queues)
                    Queues.Add(q);

                // По умолчанию выберем первую очередь (если есть)
                SelectedQueue = Queues.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Логируй/обрабатывай по необходимости
                MessageBox.Show($"Не удалось загрузить очереди: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTicketsAsync()
        {
            // Загружаем тикеты с включением связанных данных
            var tickets = await _ticketService.FindTicketsWithDetailsAsync(
                t => t.RequestorId == CurrentUser.Id,
                include: q => q
                    .Include(t => t.Requestor)
                    .Include(t => t.Supporter)
                    .Include(t => t.Queue)
            );

            // Очищаем коллекции
            Tickets.Clear();
            OpenTickets.Clear();
            ClosedTickets.Clear();

            // Создаем список задач для параллельной обработки
            var processingTasks = new List<Task>();

            foreach (var ticket in tickets)
            {
                // Добавляем тикет только ОДИН раз
                Tickets.Add(ticket);

                // Для закрытых тикетов загружаем исполнителя асинхронно
                if (ticket.Status is TicketStatus.InProgress or TicketStatus.Closed)
                {
                    if (ticket.SupporterId != null)
                    {
                        processingTasks.Add(LoadSupporterAsync(ticket));
                    }
                }

                // Распределяем по коллекциям
                if (ticket.Status == TicketStatus.Closed)
                    ClosedTickets.Add(ticket);
                else
                    OpenTickets.Add(ticket);
            }

            // Ожидаем завершения всех асинхронных операций
            await Task.WhenAll(processingTasks);

            // Настройка группировки
            GroupedTickets = CollectionViewSource.GetDefaultView(Tickets);
            GroupedTickets.GroupDescriptions.Clear();
            GroupedTickets.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Ticket.Status)));
        }

        private async Task LoadSupporterAsync(Ticket ticket)
        {
            var supporter = await _userService.GetUserByIdAsync(ticket.SupporterId.Value);
            ticket.Supporter = supporter;
        }

        [RelayCommand]
        private async void CreateNewTicket()
        {
            if (SelectedQueue == null)
            {
                MessageBox.Show("Пожалуйста, выберите очередь.", "Не выбрана очередь", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Minsk"); // Windows на Linux/Mac может быть "Europe/Moscow"
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            var newTicket = new Ticket
            {
                Status = TicketStatus.Active,
                Created = local,
                LastUpdated = local,               
                QueueId = SelectedQueue.Id,        // <-- используем выбранную очередь
                RequestorId = CurrentUser.Id,
                SupporterId = null,
                //Subject = this.Subject,           // предполагается, что Subject из биндинга в форме
                //Content = this.Body               // Body — текст из RTE
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
            await LoadTicketsAsync();
        }

        // Вспомогательный проверочный метод (чтобы не забыть _authService в коде)
        private void _auth_service_check(IAuthService auth) { /* no-op, просто для соблюдения порядка параметров */ }
    }
}