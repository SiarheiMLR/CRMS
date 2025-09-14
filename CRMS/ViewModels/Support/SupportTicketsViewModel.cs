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
using System.Windows.Documents;                                         // PageContent, FixedPage, FixedDocument...
using System.IO;
using System.Text;
using CRMS.Views.User;
using System.Globalization;
using Microsoft.Win32;
using System.Xaml;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using CRMS.Business.Services.MessageService;
using MaterialDesignThemes.Wpf;
using CommunityToolkit.Mvvm.Messaging;
using CRMS.Business.Services.DocumentService;
using CRMS.Business.Services.EmailService;
using CRMS.Business.Services.EmailService.Templates;
using System.Windows.Input;
using System.Windows.Media;
using PdfSharp.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using CRMS.Views.Support;
using Microsoft.Extensions.Logging;
using CRMS.Helpers;

namespace CRMS.ViewModels.Support
{
    public partial class SupportTicketsViewModel : ObservableObject
    {
        public enum ViewType
        {
            MyTickets,
            NewTickets,
            ClosedTickets,
            Stats
        }

        private readonly ITicketService _ticketService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;
        private readonly IDocumentConverter _documentConverter;
        private readonly IEmailService _emailService;
        private readonly IMessenger _messenger;
        private readonly IMessageService _messageService;
        private readonly ILogger<SupportTicketsViewModel> _logger;

        private ICollectionView _ticketsView;
        private readonly AsyncRelayCommand _takeTicketCommand;
        private readonly AsyncRelayCommand _addCommentDialogCommand; // ручной командный объект

        public IAsyncRelayCommand TakeTicketCommand => _takeTicketCommand;
        public IAsyncRelayCommand AddCommentDialogCommand => _addCommentDialogCommand;
        public IAsyncRelayCommand AddCommentCommand => _addCommentDialogCommand;

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

        // Таймер для обновления времени в реальном времени
        private DispatcherTimer _timer;

        // ==== Коллекции для фильтров ====
        public ObservableCollection<FilterOption<TicketStatus>> Statuses { get; } = new();
        public ObservableCollection<FilterOption<TicketPriority>> Priorities { get; } = new();
        public ObservableCollection<FilterOptionRef<Queue>> Queues { get; } = new();
        public ObservableCollection<FilterOptionRef<User>> Creators { get; } = new();

        // ==== Коллекция всех заявок для отображения ====
        [ObservableProperty]
        private ObservableCollection<Ticket> _tickets = new();

        // ==== Выбранные значения фильтров ====
        [ObservableProperty]
        private FilterOption<TicketStatus> _selectedStatus;
        [ObservableProperty]
        private FilterOption<TicketPriority> _selectedPriority;
        [ObservableProperty]
        private FilterOptionRef<Queue> _selectedQueue;
        [ObservableProperty]
        private FilterOptionRef<User> _selectedCreator;

        // Отфильтрованная коллекция
        [ObservableProperty]
        private ObservableCollection<Ticket> _filteredTickets = new();

        // Исходная коллекция всех заявок
        private ObservableCollection<Ticket> _allTickets = new();

        partial void OnSelectedStatusChanged(FilterOption<TicketStatus> value) => ApplyFilters();
        partial void OnSelectedPriorityChanged(FilterOption<TicketPriority> value) => ApplyFilters();
        partial void OnSelectedQueueChanged(FilterOptionRef<Queue> value) => ApplyFilters();
        partial void OnSelectedCreatorChanged(FilterOptionRef<User> value) => ApplyFilters();

        // Для реализации единой ViewModel с фильтрацией
        [ObservableProperty]
        private ViewType _currentView;

        // Выбранная заявка
        [ObservableProperty]
        private Ticket _selectedTicket;

        // Коллекция комментариев для выбранной заявки (ObservableCollection -> UI оповещается)
        [ObservableProperty]
        private ObservableCollection<TicketComment> _selectedTicketComments = new();

        // Новый комментарий (FlowDocument) привязан к диалогу
        [ObservableProperty]
        private FlowDocument _newCommentDocument = new FlowDocument(new Paragraph());

        // Свойства для отображения времени
        [ObservableProperty]
        private string _startedAtHeader;

        [ObservableProperty]
        private string _startedAtTimeValue;

        [ObservableProperty]
        private Brush _startedAtTimeColor;

        [ObservableProperty]
        private string _completedAtHeader;

        [ObservableProperty]
        private string _completedAtTimeValue;

        [ObservableProperty]
        private Brush _completedAtTimeColor;

        // Свойство для отображения/скрытия меню изменения статуса
        [ObservableProperty]
        private bool _isStatusMenuOpen;

        // Таймеры для обратного отсчета SLA
        private DispatcherTimer _responseTimer;
        private DispatcherTimer _resolutionTimer;

        // Конструктор класса
        public SupportTicketsViewModel(ILogger<SupportTicketsViewModel> logger, ITicketService ticketService, IAuthService authService,
            IUserService userService, IQueueService queueService, IEmailService emailService,
            IDocumentConverter documentConverter, IMessageService messageService, IMessenger messenger = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Проверка на режим дизайна
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _ticketService = ticketService;
            _auth_service_check(authService); // helper below
            _authService = authService;
            _userService = userService;
            _queue_service_check(queueService); // no-op if you wish
            _queueService = queueService;
            _documentConverter = documentConverter;
            _emailService = emailService;
            _messageService = messageService;
            _messenger = messenger;

            _ticketsView = CollectionViewSource.GetDefaultView(FilteredTickets);
            _ticketsView.Filter = TicketFilter;

            // Инициализация таймеров
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Инициализация таймеров SLA
            _responseTimer = new DispatcherTimer();
            _responseTimer.Interval = TimeSpan.FromSeconds(1);
            _responseTimer.Tick += ResponseTimer_Tick;

            _resolutionTimer = new DispatcherTimer();
            _resolutionTimer.Interval = TimeSpan.FromSeconds(1);
            _resolutionTimer.Tick += ResolutionTimer_Tick;

            // Инициализация свойств времени
            StartedAtTimeColor = Brushes.LimeGreen;
            CompletedAtTimeColor = Brushes.LimeGreen;
            StartedAtTimeValue = " ";
            CompletedAtTimeValue = " ";

            // делаем асинхронную инициализацию
            _ = InitializeAsync();

            // Инициализация команды взятия заявки
            _takeTicketCommand = new AsyncRelayCommand(ExecuteTakeTicketAsync, () => CanTakeTicket());
            _addCommentDialogCommand = new AsyncRelayCommand(ExecuteOpenAddCommentDialogAsync, () => CanAddComment);

            // инициализация SelectedTicketComments пустой коллекцией
            SelectedTicketComments = new ObservableCollection<TicketComment>();

            _logger.LogInformation("SupportTicketsViewModel initialized");
        }

        private async Task InitializeAsync()
        {
            // сначала фильтры
            await InitializeFiltersAsync();

            // потом тикеты
            await LoadTickets();

            // только теперь ставим дефолтное отображение
            CurrentView = ViewType.MyTickets;
            ApplyFilters();
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

                // Авторы - только те, кто создал хотя бы одну заявку
                Creators.Add(new FilterOptionRef<User>(null, "Все пользователи"));

                // Загружаем все заявки, чтобы получить список авторов
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

                // Устанавливаем значения по умолчанию
                SelectedStatus = Statuses.First();
                SelectedPriority = Priorities.First();
                SelectedQueue = Queues.First();
                SelectedCreator = Creators.First();
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка загрузки фильтров: {ex.Message}");
            }
        }

        private bool TicketFilter(object item)
        {
            var ticket = item as Ticket;
            if (ticket == null) return false;

            return (SelectedStatus == null || !SelectedStatus.Value.HasValue || ticket.Status == SelectedStatus.Value.Value) &&
                   (SelectedPriority == null || !SelectedPriority.Value.HasValue || ticket.Priority == SelectedPriority.Value.Value) &&
                   (SelectedQueue == null || SelectedQueue.Value == null || ticket.QueueId == SelectedQueue.Value.Id) &&
                   (SelectedCreator == null || SelectedCreator.Value == null || ticket.RequestorId == SelectedCreator.Value.Id);
        }

        partial void OnCurrentViewChanged(ViewType value)
        {
            switch (value)
            {
                case ViewType.MyTickets:
                    SelectedStatus = Statuses.FirstOrDefault(s => s.Value == TicketStatus.InProgress);
                    break;

                case ViewType.NewTickets:
                    SelectedStatus = Statuses.FirstOrDefault(s => s.Value == TicketStatus.Active);
                    break;

                case ViewType.ClosedTickets:
                    SelectedStatus = Statuses.FirstOrDefault(s => s.Value == TicketStatus.Closed);
                    break;

                default:
                    // оставляем как есть (можно сбросить на "All statuses")
                    SelectedStatus = Statuses.FirstOrDefault();
                    break;
            }

            ApplyFilters();
        }

        public void ApplyFilters()
        {
            if (_allTickets.Count == 0) return;

            IEnumerable<Ticket> filtered = _allTickets;

            switch (CurrentView)
            {
                case ViewType.MyTickets:
                    filtered = filtered.Where(t =>
                        t.SupporterId == _authService.CurrentUser.Id &&
                        t.Status == TicketStatus.InProgress);
                    break;

                case ViewType.NewTickets:
                    filtered = filtered.Where(t =>
                        t.SupporterId == null &&
                        t.Status == TicketStatus.Active);
                    break;

                case ViewType.ClosedTickets:
                    filtered = filtered.Where(t =>
                        t.SupporterId == _authService.CurrentUser.Id &&
                        t.Status == TicketStatus.Closed);
                    break;
            }

            filtered = filtered.Where(ticket =>
                (SelectedStatus == null || !SelectedStatus.Value.HasValue || ticket.Status == SelectedStatus.Value.Value) &&
                (SelectedPriority == null || !SelectedPriority.Value.HasValue || ticket.Priority == SelectedPriority.Value.Value) &&
                (SelectedQueue == null || SelectedQueue.Value == null || ticket.QueueId == SelectedQueue.Value.Id) &&
                (SelectedCreator == null || SelectedCreator.Value == null || ticket.RequestorId == SelectedCreator.Value.Id)
            );

            FilteredTickets = new ObservableCollection<Ticket>(filtered);
        }

        // Команда для загрузки заявок
        [RelayCommand]
        private async Task LoadTickets()
        {
            try
            {
                // Загружаем заявки с включением связанных данных
                var tickets = await _ticketService.GetAllTicketsWithDetailsAsync();
                _allTickets = new ObservableCollection<Ticket>(tickets);
                ApplyFilters(); // Применяем фильтры после загрузки                
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка загрузки заявок: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ResetFilters()
        {
            SelectedStatus = Statuses.FirstOrDefault();
            SelectedPriority = Priorities.FirstOrDefault();
            SelectedQueue = Queues.FirstOrDefault();
            SelectedCreator = Creators.FirstOrDefault();
        }

        // Таймеры для SLA    
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTimeProperties();
        }

        // Таймер для обратного отсчета времени ответа
        private void ResponseTimer_Tick(object sender, EventArgs e)
        {
            UpdateRemainingTimeForStartedAt();
        }

        // Таймер для обратного отсчета времени решения
        private void ResolutionTimer_Tick(object sender, EventArgs e)
        {
            UpdateRemainingTimeForCompletedAt();
        }

        partial void OnSelectedTicketChanged(Ticket value)
        {
            if (value != null)
            {
                // Останавливаем предыдущие таймеры
                _responseTimer.Stop();
                _resolutionTimer.Stop();

                UpdateTimeProperties();

                // Запускаем соответствующие таймеры
                if (value.StartedAt == null)
                {
                    _responseTimer.Start();
                }

                if (value.StartedAt != null && value.CompletedAt == null)
                {
                    _resolutionTimer.Start();
                }

                // Обновляем состояние команды при изменении выбранной заявки
                _takeTicketCommand?.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanChangeStatus));
                ChangeStatusCommand.NotifyCanExecuteChanged();

                // Обновляем доступность кнопки "Добавить комментарий"
                _addCommentDialogCommand?.NotifyCanExecuteChanged();

                // !!! Обновляем свойство CanAddComment чтобы привязки IsEnabled увидели изменение
                OnPropertyChanged(nameof(CanAddComment));

                // Загружаем комментарии асинхронно (чтобы гарантированно подтянуть User)
                _ = LoadCommentsForSelectedTicketAsync(value?.Id);
            }
            else
            {
                // Останавливаем таймеры при отсутствии выбранной заявки
                _responseTimer.Stop();
                _resolution_timer_stop_silently();
                // Сброс значений при отсутствии выбранной заявки
                StartedAtTimeValue = " ";
                CompletedAtTimeValue = " ";

                // Обновляем состояние команды
                _takeTicketCommand?.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanChangeStatus));
                ChangeStatusCommand.NotifyCanExecuteChanged();
                _addCommentDialogCommand?.NotifyCanExecuteChanged();

                // Обновляем привязку CanAddComment
                OnPropertyChanged(nameof(CanAddComment));

                SelectedTicketComments.Clear();
            }
        }

        // Небольшая обёртка, чтобы не ругался анализатор
        private void _resolution_timer_stop_silently()
        {
            try { _resolutionTimer.Stop(); } catch { }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} д {timeSpan.Hours} ч {timeSpan.Minutes} мин";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} ч {timeSpan.Minutes} мин";
            return $"{timeSpan.Minutes} мин {timeSpan.Seconds} сек";
        }

        private void UpdateTimeProperties()
        {
            if (SelectedTicket == null)
                return;

            // Обновление времени для взятия на выполнение
            if (SelectedTicket.StartedAt == null)
            {
                StartedAtHeader = "Осталось времени:";
                UpdateRemainingTimeForStartedAt();
            }
            else
            {
                StartedAtHeader = "Прошло времени:";
                UpdateElapsedTimeForStartedAt();
            }

            // Обновление времени для завершения
            if (SelectedTicket.CompletedAt == null)
            {
                CompletedAtHeader = "Осталось времени:";
                UpdateRemainingTimeForCompletedAt();
            }
            else
            {
                CompletedAtHeader = "Прошло времени:";
                UpdateElapsedTimeForCompletedAt();
            }
        }

        private void UpdateRemainingTimeForStartedAt()
        {
            if (SelectedTicket?.Queue == null)
            {
                StartedAtTimeValue = " Нет данных";
                return;
            }

            var responseTime = SelectedTicket.Queue.SlaResponseTime ?? TimeSpan.Zero;
            if (responseTime == TimeSpan.Zero)
            {
                StartedAtTimeValue = " Нет данных";
                return;
            }

            var deadline = SelectedTicket.Created + responseTime;
            var remaining = deadline - DateTime.Now;

            if (remaining <= TimeSpan.Zero)
            {
                StartedAtTimeValue = " Превышен срок!";
                StartedAtTimeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8C00")); // Темно-оранжевый

                // Останавливаем таймер, если время истекло
                _responseTimer.Stop();
            }
            else
            {
                // Используем кастомный формат для отображения общего количества часов
                StartedAtTimeValue = " " + FormatTimeSpan(remaining);
                StartedAtTimeColor = remaining <= responseTime * 0.25 ? Brushes.Orange : Brushes.LimeGreen;
            }
        }

        private void UpdateElapsedTimeForStartedAt()
        {
            if (SelectedTicket?.StartedAt == null || SelectedTicket?.Created == null)
            {
                StartedAtTimeValue = " Нет данных";
                return;
            }

            var elapsed = SelectedTicket.StartedAt.Value - SelectedTicket.Created;
            var responseTime = SelectedTicket.Queue?.SlaResponseTime ?? TimeSpan.Zero;

            // Используем кастомный формат вместо стандартного
            StartedAtTimeValue = " " + FormatTimeSpan(elapsed);

            if (responseTime == TimeSpan.Zero)
            {
                StartedAtTimeColor = Brushes.LimeGreen;
            }
            else if (elapsed > responseTime)
            {
                StartedAtTimeColor = Brushes.Red;
            }
            else
            {
                StartedAtTimeColor = elapsed <= responseTime * 0.25 ? Brushes.LimeGreen : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8C00"));
            }
        }

        private void UpdateRemainingTimeForCompletedAt()
        {
            if (SelectedTicket?.Queue == null)
            {
                CompletedAtTimeValue = " Нет данных";
                return;
            }

            var resolutionTime = SelectedTicket.Queue.SlaResolutionTime ?? TimeSpan.Zero;
            if (resolutionTime == TimeSpan.Zero)
            {
                CompletedAtTimeValue = " Нет данных";
                return;
            }

            // Если заявка не взята в работу, но срок ответа превышен,
            // то и срок решения тоже превышен (красный цвет)
            if (SelectedTicket.StartedAt == null)
            {
                // Проверяем, превышен ли срок ответа
                var responseTime = SelectedTicket.Queue.SlaResponseTime ?? TimeSpan.Zero;
                if (responseTime != TimeSpan.Zero)
                {
                    var responseDeadline = SelectedTicket.Created + responseTime;
                    if (DateTime.Now > responseDeadline)
                    {
                        CompletedAtTimeValue = " Превышен срок!";
                        CompletedAtTimeColor = Brushes.Red;
                        return;
                    }
                }

                CompletedAtTimeValue = " Нет данных";
                return;
            }

            var deadline = SelectedTicket.StartedAt.Value + resolutionTime;
            var remaining = deadline - DateTime.Now;

            if (remaining <= TimeSpan.Zero)
            {
                CompletedAtTimeValue = " Превышен срок!";
                CompletedAtTimeColor = Brushes.Red;
                _resolutionTimer.Stop();
            }
            else
            {
                CompletedAtTimeValue = " " + FormatTimeSpan(remaining);
                CompletedAtTimeColor = remaining <= resolutionTime * 0.25 ? Brushes.Orange : Brushes.LimeGreen;
            }
        }

        private void UpdateElapsedTimeForCompletedAt()
        {
            if (SelectedTicket?.CompletedAt == null || SelectedTicket?.StartedAt == null)
            {
                CompletedAtTimeValue = " Нет данных";
                return;
            }

            var elapsed = SelectedTicket.CompletedAt.Value - SelectedTicket.StartedAt.Value;
            var resolutionTime = SelectedTicket.Queue?.SlaResolutionTime ?? TimeSpan.Zero;

            // Используем кастомный формат вместо стандартного
            CompletedAtTimeValue = " " + FormatTimeSpan(elapsed);

            if (resolutionTime == TimeSpan.Zero)
            {
                CompletedAtTimeColor = Brushes.LimeGreen;
            }
            else if (elapsed > resolutionTime)
            {
                CompletedAtTimeColor = Brushes.Red;
            }
            else
            {
                CompletedAtTimeColor = elapsed <= resolutionTime * 0.25 ? Brushes.LimeGreen : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8C00"));
            }
        }

        // Команда для скачивания вложения
        [RelayCommand]
        private void DownloadAttachment(Attachment attachment)
        {
            if (attachment == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                FileName = attachment.FileName,
                Filter = $"Файлы (*{Path.GetExtension(attachment.FileName)})|*{Path.GetExtension(attachment.FileName)}|Все файлы (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllBytes(saveFileDialog.FileName, attachment.FileData);
                    _messageService.ShowInfo($"Файл {attachment.FileName} успешно сохранен");
                }
                catch (Exception ex)
                {
                    _messageService.ShowError($"Ошибка при сохранении файла: {ex.Message}");
                }
            }
        }

        // Проверка возможности выполнения команды взятия заявки
        private bool CanTakeTicket()
        {
            return SelectedTicket != null &&
                   SelectedTicket.Status == TicketStatus.Active &&
                   _authService.CurrentUser != null;
        }

        // Выполнение команды взятия заявки
        private async Task ExecuteTakeTicketAsync()
        {
            if (SelectedTicket == null || _authService.CurrentUser == null)
                return;

            try
            {
                if (!_messageService.ShowConfirmation($"Вы уверены, что хотите взять заявку {SelectedTicket.TicketNumber}?", "Подтверждение"))
                    return;

                // вызываем бизнес-логику через сервис
                await _ticketService.AssignTicket(SelectedTicket, _authService.CurrentUser.Id);

                // Создаем транзакцию
                await _ticket_service_add_transaction_check(SelectedTicket.Id, _authService.CurrentUser.Id, "Take", "Заявка взята в работу!");

                _messageService.ShowInfo($"Заявка {SelectedTicket.TicketNumber} взята вами на выполнение!");

                // Останавливаем таймер ответа и запускаем таймер решения
                _responseTimer.Stop();
                if (SelectedTicket.CompletedAt == null)
                {
                    _resolutionTimer.Start();
                }

                // Обновляем состояние команды
                _takeTicketCommand.NotifyCanExecuteChanged();

                // Перезагружаем список заявок из базы
                await LoadTickets();
                OnPropertyChanged(nameof(FilteredTickets));

                // Обновляем время таймеров SLA
                UpdateTimeProperties();
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка при взятии заявки в работу: {ex.Message}");
            }
        }

        // Вспомогательный метод для создания транзакции
        private async Task _ticket_service_add_transaction_check(int ticketId, int userId, string actionType, string details)
        {
            try
            {
                await _ticketService.AddTransactionAsync(new Transaction
                {
                    TicketId = ticketId,
                    UserId = userId,
                    ActionType = actionType,
                    Details = details,
                    Created = DateTime.Now
                });
            }
            catch { /* ignore transaction add errors here if needed */ }
        }

        // Команда для открытия/закрытия меню изменения статуса
        [RelayCommand]
        private void ToggleStatusMenu()
        {
            IsStatusMenuOpen = !IsStatusMenuOpen;
        }

        // Команда для закрытия заявки
        [RelayCommand]
        private async Task CloseTicket()
        {
            if (SelectedTicket == null || SelectedTicket.Status != TicketStatus.InProgress) return;

            try
            {
                if (!_messageService.ShowConfirmation($"Вы уверены, что хотите закрыть заявку {SelectedTicket.TicketNumber}?", "Подтверждение"))
                    return;

                // Сохраняем информацию о закрытии заявки
                await _ticket_service_add_transaction_check(SelectedTicket.Id, _authService.CurrentUser?.Id ?? 0, "Close", "Закрытие заявки");

                SelectedTicket.Status = TicketStatus.Closed;
                SelectedTicket.CompletedAt = DateTime.Now;

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _messageService.ShowInfo($"Заявка {SelectedTicket.TicketNumber} закрыта!");

                // Останавливаем таймер решения
                _resolutionTimer.Stop();

                // В методе CloseTicket после успешного закрытия заявки:
                await SendStatusChangeNotifications(TicketStatus.Closed, "Заявка была закрыта.");

                IsStatusMenuOpen = false;
                await LoadTickets();
                OnPropertyChanged(nameof(FilteredTickets));

                // Обновляем время таймеров SLA
                UpdateTimeProperties();
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка при закрытии заявки: {ex.Message}");
            }
        }

        // Команда для отказа от заявки
        [RelayCommand]
        private async Task UnassignTicket()
        {
            if (SelectedTicket == null || SelectedTicket.Status != TicketStatus.InProgress) return;

            try
            {
                if (!_messageService.ShowConfirmation($"Вы уверены, что хотите отказаться от заявки {SelectedTicket.TicketNumber}?", "Подтверждение"))
                    return;

                // Сохраняем информацию об отказе от выполнения заявки
                await _ticket_service_add_transaction_check(SelectedTicket.Id, _authService.CurrentUser?.Id ?? 0, "Unassign", "Отказ от выполнения заявки!");

                SelectedTicket.Status = TicketStatus.Active;
                SelectedTicket.SupporterId = null;
                SelectedTicket.StartedAt = null;

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _messageService.ShowInfo($"Отказ от заявки {SelectedTicket.TicketNumber} оформлен!");

                // Останавливаем таймер решения и запускаем таймер ответа
                _resolutionTimer.Stop();
                _responseTimer.Start();

                // В методе UnassignTicket после успешного отказа от заявки:
                await SendStatusChangeNotifications(TicketStatus.Active, "Исполнитель отказался от заявки.");

                IsStatusMenuOpen = false;
                await LoadTickets();
                OnPropertyChanged(nameof(FilteredTickets));

                // Обновляем время таймеров SLA
                UpdateTimeProperties();
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка при отказе от заявки: {ex.Message}");
            }
        }

        // Команда для открытия заявки
        [RelayCommand]
        private async Task ReopenTicket()
        {
            if (SelectedTicket == null || SelectedTicket.Status != TicketStatus.Closed) return;

            try
            {
                if (!_messageService.ShowConfirmation($"Вы уверены, что хотите открыть заново заявку {SelectedTicket.TicketNumber}?", "Подтверждение"))
                    return;

                // Сохраняем информацию о повторном открытии заявки
                await _ticket_service_add_transaction_check(SelectedTicket.Id, _authService.CurrentUser?.Id ?? 0, "Reopen", "Повторное открытие заявки!");

                SelectedTicket.Status = TicketStatus.Active;
                SelectedTicket.SupporterId = null;
                SelectedTicket.StartedAt = null;
                SelectedTicket.CompletedAt = null;

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _messageService.ShowInfo($"Заявка {SelectedTicket.TicketNumber} заново открыта!");

                // Запускаем таймер ответа
                _responseTimer.Start();

                // В методе ReopenTicket после успешного reopening заявки:
                await SendStatusChangeNotifications(TicketStatus.Active, "Заявка была открыта повторно.");

                // В методе ReopenTicket после успешного reopening заявки:
                await SendTicketReopenedNotifications("Заявка была открыта повторно.");

                IsStatusMenuOpen = false;
                await LoadTickets();
                OnPropertyChanged(nameof(FilteredTickets));

                // Обновляем время таймеров SLA
                UpdateTimeProperties();
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка при открытии заявки: {ex.Message}");
            }
        }

        // Свойство для проверки активности кнопки "Изменить статус"
        public bool CanChangeStatus => SelectedTicket != null &&
            (SelectedTicket.Status == TicketStatus.InProgress || SelectedTicket.Status == TicketStatus.Closed);

        // Команда для изменения статуса
        [RelayCommand]
        private async Task ChangeStatus()
        {
            if (SelectedTicket == null) return;

            try
            {
                if (SelectedTicket.Status == TicketStatus.Closed)
                {
                    if (!_messageService.ShowConfirmation($"Вы уверены, что хотите открыть заново заявку {SelectedTicket.TicketNumber}?", "Подтверждение"))
                        return;

                    SelectedTicket.Status = TicketStatus.Active;
                    SelectedTicket.CompletedAt = null;

                    // Запускаем таймер ответа
                    _responseTimer.Start();
                }
                else if (SelectedTicket.Status == TicketStatus.Active)
                {
                    if (!_messageService.ShowConfirmation($"Вы уверены, что хотите взять заявку {SelectedTicket.TicketNumber}?", "Подтверждение"))
                        return;

                    SelectedTicket.Status = TicketStatus.InProgress;
                    SelectedTicket.StartedAt = DateTime.Now;

                    // Останавливаем таймер ответа и запускаем таймер решения
                    _responseTimer.Stop();
                    _resolutionTimer.Start();
                }
                else if (SelectedTicket.Status == TicketStatus.InProgress)
                {
                    if (!_messageService.ShowConfirmation($"Вы уверены, что хотите закрыть заявку {SelectedTicket.TicketNumber}?", "Подтверждение"))
                        return;

                    SelectedTicket.Status = TicketStatus.Closed;
                    SelectedTicket.CompletedAt = DateTime.Now;

                    // Останавливаем таймер решения
                    _resolutionTimer.Stop();
                }

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _messageService.ShowInfo($"Статус заявки {SelectedTicket.TicketNumber} изменен на {SelectedTicket.Status}!");

                // В методе ChangeStatus после успешного изменения статуса:
                await SendStatusChangeNotifications(SelectedTicket.Status, "Статус заявки был изменен.");

                await LoadTickets();
                OnPropertyChanged(nameof(FilteredTickets));
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка при изменении статуса: {ex.Message}");
            }
        }

        // ==== Команды для работы с комментариями ====

        // Генератор флага для возможности добавлять комментарий
        public bool CanAddComment =>
            SelectedTicket != null &&
            SelectedTicket.Status == TicketStatus.InProgress &&
            SelectedTicket.SupporterId == _authService.CurrentUser?.Id;

        // Команда открытия диалога добавления комментария
        private async Task ExecuteOpenAddCommentDialogAsync()
        {
            if (SelectedTicket == null)
            {
                _messageService.ShowError("Не выбрана заявка для добавления комментария!");
                return;
            }

            try
            {
                // Сбрасываем документ перед открытием
                NewCommentDocument = new FlowDocument(new Paragraph());

                var dlg = new Views.Support.AddCommentDialog();
                dlg.DataContext = this;
                dlg.ResetEditor();

                // Показываем диалог
                await DialogHost.Show(dlg, "TicketDetailsDialog");
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка открытия диалога: {ex.Message}");
            }
        }

        // Команда подтверждения добавления комментария (генерируется из атрибута)
        [RelayCommand]
        private async Task AddCommentConfirm(RichTextEditor editor)
        {
            if (SelectedTicket == null || editor == null)
                return;

            // Используем документ из редактора
            FlowDocument document = editor.Document;

            // Правильная проверка содержимого
            if (!HasContent(document))
            {
                _messageService.ShowError("Комментарий не может быть пустым!");
                return;
            }

            // Нормализуем изображения, как при создании тикета
            try
            {
                Ticket.NormalizeImagesInFlowDocument(document);
            }
            catch
            {
                // если нормализация не удалась — всё равно пытаемся сохранить текст
            }

            // Конвертируем в XAML
            string xaml = Ticket.ConvertFlowDocumentToXaml(document);

            // Создаём новый комментарий
            var comment = new TicketComment
            {
                TicketId = SelectedTicket.Id,
                UserId = _authService.CurrentUser.Id,
                // Created будет установлен в сервисе
                Content = xaml,
                IsInternal = false
            };

            try
            {
                // Отправляем на сервер/в сервис
                await _ticketService.AddCommentAsync(comment);

                // Обновляем UI: перечитываем список комментариев из сервиса
                await LoadCommentsForSelectedTicketAsync(SelectedTicket.Id);

                _messageService.ShowInfo("Комментарий успешно добавлен!");

                // В методе AddCommentConfirm после успешного добавления комментария:
                await SendCommentNotifications(comment, document);

                // Для обновления комментариев у пользоватля
                _messenger?.Send(new CommentAddedMessage
                {
                    TicketId = comment.TicketId,
                    Comment = comment
                });
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка при добавлении комментария: {ex.Message}");
            }

            // Сбрасываем состояние
            editor.Document = new FlowDocument(new Paragraph());
            NewCommentDocument = new FlowDocument(new Paragraph());

            // Закрываем диалог
            DialogHost.Close("TicketDetailsDialog");
        }

        //Вспомогательный метод для проверки содержимого:
        private bool HasContent(FlowDocument document)
        {
            if (document == null) return false;

            // Проверяем текстовое содержимое
            string text = new TextRange(document.ContentStart, document.ContentEnd).Text;
            if (!string.IsNullOrWhiteSpace(text))
                return true;

            // Проверяем наличие изображений или других элементов
            foreach (Block block in document.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is InlineUIContainer)
                            return true;

                        if (inline is Run run && !string.IsNullOrWhiteSpace(run.Text))
                            return true;
                    }
                }
                else if (block is Section section)
                {
                    // Рекурсивно проверяем секции
                    if (HasContent(new FlowDocument(section))) return true;
                }
            }

            return false;
        }

        // Асинхронно загружает комментарии и кладёт их в SelectedTicketComments
        private async Task LoadCommentsForSelectedTicketAsync(int? ticketId)
        {
            if (ticketId == null)
            {
                SelectedTicketComments = new ObservableCollection<TicketComment>();
                return;
            }

            try
            {
                var comments = (await _ticketService.GetCommentsByTicketIdAsync(ticketId.Value)).ToList();

                // Обновляем коллекцию (через замену, чтобы привязки заметили изменение)
                SelectedTicketComments = new ObservableCollection<TicketComment>(comments);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка загрузки комментариев: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Save()
        {
            // Проверяем, есть ли текст в текущем документе комментария
            string documentText = new TextRange(NewCommentDocument.ContentStart, NewCommentDocument.ContentEnd).Text;
            if (string.IsNullOrWhiteSpace(documentText))
            {
                _messageService.ShowError("Редактор не содержит текста. Нечего сохранять.");
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "Rich Text Format (*.rtf)|*.rtf|PDF Files (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // Готовим клон для экспорта
                    var exportDoc = PrepareDocumentForExport(NewCommentDocument);

                    if (System.IO.Path.GetExtension(dlg.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        SaveAsPdf(dlg.FileName, exportDoc);
                    }
                    else
                    {
                        var range = new TextRange(exportDoc.ContentStart, exportDoc.ContentEnd);
                        using var fs = new FileStream(dlg.FileName, FileMode.Create);
                        range.Save(fs, DataFormats.Rtf);
                    }

                    _messageService.ShowInfo("Документ успешно сохранён!");
                }
                catch (Exception ex)
                {
                    _messageService.ShowError($"Ошибка при сохранении: {ex.Message}");
                }
            }
        }

        private FlowDocument PrepareDocumentForExport(FlowDocument source)
        {
            var clone = CloneFlowDocument(source);

            // Применяем ко всему документу чёрный цвет
            var range = new TextRange(clone.ContentStart, clone.ContentEnd);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);

            return clone;
        }

        private FlowDocument CloneFlowDocument(FlowDocument source)
        {
            if (source == null) return new FlowDocument();

            var range = new TextRange(source.ContentStart, source.ContentEnd);

            using var ms = new MemoryStream();
            range.Save(ms, DataFormats.XamlPackage);

            var clone = new FlowDocument();
            var cloneRange = new TextRange(clone.ContentStart, clone.ContentEnd);
            ms.Position = 0;
            cloneRange.Load(ms, DataFormats.XamlPackage);

            return clone;
        }

        private void SaveAsPdf(string filePath, FlowDocument exportDoc)
        {
            string tempXpsPath = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), ".xps");

            try
            {
                using (var xpsDoc = new XpsDocument(tempXpsPath, FileAccess.ReadWrite))
                {
                    var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                    writer.Write(((IDocumentPaginatorSource)exportDoc).DocumentPaginator);
                    xpsDoc.Close();
                }

                ConvertXpsToPdf(tempXpsPath, filePath);
            }
            finally
            {
                try { if (File.Exists(tempXpsPath)) File.Delete(tempXpsPath); } catch { }
            }
        }

        private void ConvertXpsToPdf(string xpsPath, string pdfPath)
        {
            using var xpsDoc = new XpsDocument(xpsPath, FileAccess.Read);
            var fixedDocSeq = xpsDoc.GetFixedDocumentSequence();

            using var pdfDoc = new PdfSharp.Pdf.PdfDocument();

            const double dpi = 200.0;
            const double ptPerDip = 72.0 / 96.0;

            foreach (var docRef in fixedDocSeq.References)
            {
                var fixedDoc = docRef.GetDocument(false);
                foreach (PageContent pageContent in fixedDoc.Pages)
                {
                    var fixedPage = pageContent.GetPageRoot(false) as FixedPage;
                    if (fixedPage == null) continue;

                    fixedPage.Measure(new Size(fixedPage.Width, fixedPage.Height));
                    fixedPage.Arrange(new Rect(new Size(fixedPage.Width, fixedPage.Height)));
                    fixedPage.UpdateLayout();

                    var pdfPage = pdfDoc.AddPage();
                    pdfPage.Width = fixedPage.Width * ptPerDip;
                    pdfPage.Height = fixedPage.Height * ptPerDip;

                    string tmpPng = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), ".png");
                    try
                    {
                        int pxW = (int)Math.Ceiling(fixedPage.Width * dpi / 96.0);
                        int pxH = (int)Math.Ceiling(fixedPage.Height * dpi / 96.0);

                        var rtb = new RenderTargetBitmap(pxW, pxH, dpi, dpi, PixelFormats.Pbgra32);
                        rtb.Render(fixedPage);

                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(rtb));
                        using (var fs = new FileStream(tmpPng, FileMode.Create, FileAccess.Write))
                            encoder.Save(fs);

                        using (var gfx = XGraphics.FromPdfPage(pdfPage))
                        using (var img = XImage.FromFile(tmpPng))
                        {
                            gfx.DrawImage(img, 0, 0, pdfPage.Width, pdfPage.Height);
                        }
                    }
                    finally
                    {
                        try { if (File.Exists(tmpPng)) File.Delete(tmpPng); } catch { }
                    }
                }
            }

            pdfDoc.Save(pdfPath);
        }

        [RelayCommand]
        private void OpenEmailClient(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"mailto:{email}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    _messageService.ShowError($"Ошибка при открытии почтового клиента: {ex.Message}");
                }
            }
        }

        // Метод для получения статистики
        public async Task<IEnumerable<Transaction>> GetUserUnassignStats(int userId, DateTime startDate, DateTime endDate)
        {
            return await _ticketService.GetUserUnassignStats(userId, startDate, endDate);
        }

        public async Task<IEnumerable<Transaction>> GetUserReopenStats(int userId, DateTime startDate, DateTime endDate)
        {
            return await _ticket_service_get_user_reopen_stats(userId, startDate, endDate);
        }

        private async Task<IEnumerable<Transaction>> _ticket_service_get_user_reopen_stats(int userId, DateTime startDate, DateTime endDate)
        {
            return await _ticketService.GetUserReopenStats(userId, startDate, endDate);
        }

        // Метод для получения общей статистики по пользователю
        public async Task<Dictionary<string, int>> GetUserStats(int userId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _ticketService.GetTransactionsAsync(userId, null, startDate, endDate);

            return transactions
                .GroupBy(t => t.ActionType)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // Вспомогательные методы для работы с email
        // Вспомогательный метод для получения текста приоритета
        private string GetPriorityText(TicketPriority priority)
        {
            return priority switch
            {
                TicketPriority.Low => "Низкий",
                TicketPriority.Mid => "Средний",
                TicketPriority.High => "Высокий",
                _ => "Не указан"
            };
        }

        // Вспомогательный метод для получения цвета приоритета
        private string GetPriorityColor(TicketPriority priority)
        {
            return priority switch
            {
                TicketPriority.Low => "#008000",
                TicketPriority.Mid => "#FFA500",
                TicketPriority.High => "#FF0000",
                _ => "#000000"
            };
        }

        // Вспомогательный метод для получения текста статуса
        private string GetStatusText(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Active => "Активная",
                TicketStatus.InProgress => "В работе",
                TicketStatus.Closed => "Закрытая",
                _ => "Неизвестно"
            };
        }

        // Вспомогательный метод для получения цвета статуса
        private string GetStatusColor(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Active => "#007bff",
                TicketStatus.InProgress => "#28a745",
                TicketStatus.Closed => "#6c757d",
                _ => "#000000"
            };
        }

        // Вспомогательный метод для форматирования размера файла
        private string GetFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private async Task SendStatusChangeNotifications(TicketStatus newStatus, string changeReason)
        {
            if (SelectedTicket == null || SelectedTicket.Requestor == null || SelectedTicket.Queue == null)
                return;

            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "UserName", SelectedTicket.Requestor.DisplayName ?? "Пользователь" },
                    { "UserEmail", SelectedTicket.Requestor.Email ?? "" },
                    { "TicketNumber", SelectedTicket.TicketNumber },
                    { "Subject", SelectedTicket.Subject ?? "" },
                    { "Status", GetStatusText(newStatus) },
                    { "StatusColor", GetStatusColor(newStatus) },
                    { "OldStatus", GetStatusText(SelectedTicket.Status) },
                    { "Executor", _authService.CurrentUser?.DisplayName ?? "Система" },
                    { "ChangedAt", DateTime.Now.ToString("g") },
                    { "Comment", changeReason },
                    { "Queue", SelectedTicket.Queue.Name ?? "" }
                };

                // Уведомление пользователю
                string userBody = Templates.TicketStatusChangedToUser;
                foreach (var p in parameters)
                    userBody = userBody.Replace("{" + p.Key + "}", p.Value);

                await _emailService.SendEmailAsync(SelectedTicket.Requestor.Email,
                    $"Статус вашей заявки #{SelectedTicket.TicketNumber} изменен", userBody);

                // Уведомление поддержке
                string supportBody = Templates.TicketStatusChangedToSupport;
                foreach (var p in parameters)
                    supportBody = supportBody.Replace("{" + p.Key + "}", p.Value);

                await _emailService.SendEmailAsync(SelectedTicket.Queue.CorrespondAddress,
                    $"Статус заявки #{SelectedTicket.TicketNumber} изменен на {GetStatusText(newStatus)}", supportBody);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки уведомления об изменении статуса: {ex.Message}");
            }
        }

        private async Task SendTicketReopenedNotifications(string reason)
        {
            if (SelectedTicket == null || SelectedTicket.Requestor == null || SelectedTicket.Queue == null)
                return;

            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "UserName", SelectedTicket.Requestor.DisplayName ?? "Пользователь" },
                    { "UserEmail", SelectedTicket.Requestor.Email ?? "" },
                    { "TicketNumber", SelectedTicket.TicketNumber },
                    { "Subject", SelectedTicket.Subject ?? "" },
                    { "Executor", _authService.CurrentUser?.DisplayName ?? "Система" },
                    { "ReopenedAt", DateTime.Now.ToString("g") },
                    { "Reason", reason },
                    { "Queue", SelectedTicket.Queue.Name ?? "" },
                    { "OriginalExecutor", SelectedTicket.Supporter?.DisplayName ?? "Не назначен" },
                    { "ReopenedBy", _authService.CurrentUser?.DisplayName ?? "Система" }
                };

                // Уведомление пользователю
                string userBody = Templates.TicketReopenedToUser;
                foreach (var p in parameters)
                    userBody = userBody.Replace("{" + p.Key + "}", p.Value);

                await _emailService.SendEmailAsync(SelectedTicket.Requestor.Email,
                    $"Ваша заявка #{SelectedTicket.TicketNumber} возобновлена", userBody);

                await _emailService.SendEmailAsync(SelectedTicket.Requestor.Email,
                    $"Ваша заявка #{SelectedTicket.TicketNumber} возобновлена", userBody);

                // Уведомление поддержке
                string supportBody = Templates.TicketReopenedToSupport;
                foreach (var p in parameters)
                    supportBody = supportBody.Replace("{" + p.Key + "}", p.Value);

                await _emailService.SendEmailAsync(SelectedTicket.Queue.CorrespondAddress,
                    $"Заявка #{SelectedTicket.TicketNumber} возобновлена пользователем", supportBody);

                // Уведомление администратору и сотруднику
                string adminBody = Templates.TicketReopenedToAdmin;
                foreach (var p in parameters)
                    adminBody = adminBody.Replace("{" + p.Key + "}", p.Value);

                // Отправляем администратору (можно добавить дополнительный email)
                await _emailService.SendEmailAsync("admin@bigfirm.by",
                        $"Заявка #{SelectedTicket.TicketNumber} возобновлена сотрудником", adminBody);

                // Отправляем сотруднику, если он не текущий пользователь
                if (SelectedTicket.SupporterId != null && SelectedTicket.SupporterId != _authService.CurrentUser?.Id)
                {
                    await _emailService.SendEmailAsync(SelectedTicket.Supporter?.Email,
                        $"Заявка #{SelectedTicket.TicketNumber} возобновлена", adminBody);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки уведомления о возобновлении заявки: {ex.Message}");
            }
        }

        private async Task SendCommentNotifications(TicketComment comment, FlowDocument document)
        {
            if (SelectedTicket == null || SelectedTicket.Requestor == null || SelectedTicket.Queue == null)
                return;

            try
            {
                // Конвертируем FlowDocument комментария в HTML
                string commentHtml = _documentConverter.FlowDocumentToHtml(document);

                var parameters = new Dictionary<string, string>
                {
                    { "UserName", SelectedTicket.Requestor.DisplayName ?? "Пользователь" },
                    { "UserEmail", SelectedTicket.Requestor.Email ?? "" },
                    { "TicketNumber", SelectedTicket.TicketNumber },
                    { "Subject", SelectedTicket.Subject ?? "" },
                    { "CommentAuthor", _authService.CurrentUser?.DisplayName ?? "Система" },
                    { "CommentDate", comment.Created.ToString("g") },
                    { "CommentText", commentHtml },
                    { "Queue", SelectedTicket.Queue.Name ?? "" }
                };

                // Уведомление пользователю
                string userBody = Templates.TicketCommentAddedToUser;
                foreach (var p in parameters)
                    userBody = userBody.Replace("{" + p.Key + "}", p.Value);

                await _emailService.SendEmailAsync(SelectedTicket.Requestor.Email,
                        $"Добавлен новый комментарий к вашей заявке #{SelectedTicket.TicketNumber}", userBody);

                // Уведомление поддержке
                string supportBody = Templates.TicketCommentAddedToSupport;
                foreach (var p in parameters)
                    supportBody = supportBody.Replace("{" + p.Key + "}", p.Value);

                await _emailService.SendEmailAsync(SelectedTicket.Queue.CorrespondAddress,
                    $"Добавлен комментарий к заявке #{SelectedTicket.TicketNumber}", supportBody);

                // Уведомление автору комментария (если это не пользователь и не адрес очереди)
                if (_authService.CurrentUser != null &&
                    _authService.CurrentUser.Id != SelectedTicket.RequestorId &&
                    _authService.CurrentUser.Email != SelectedTicket.Queue.CorrespondAddress)
                {
                    string authorBody = Templates.TicketCommentAddedToAuthor;
                    foreach (var p in parameters)
                        authorBody = authorBody.Replace("{" + p.Key + "}", p.Value);

                    await _emailService.SendEmailAsync(_authService.CurrentUser.Email,
                        $"Ваш комментарий к заявке #{SelectedTicket.TicketNumber} добавлен", authorBody);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки уведомления о комментарии: {ex.Message}");
            }
        }

        // Вспомогательный проверочный метод (чтобы не забыть _authService в коде)
        private void _auth_service_check(IAuthService auth) { /* no-op, просто для соблюдения порядка параметров */ }
        private void _queue_service_check(IQueueService queue) { /* no-op */ }
    }
}
