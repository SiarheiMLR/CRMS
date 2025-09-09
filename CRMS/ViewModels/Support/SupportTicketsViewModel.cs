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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Windows.Threading;
using CRMS.Business.Services.MessageService;

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
        private ICollectionView _ticketsView;
        private readonly AsyncRelayCommand _takeTicketCommand;
        public IAsyncRelayCommand TakeTicketCommand => _takeTicketCommand;

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
        public SupportTicketsViewModel(ITicketService ticketService, IAuthService authService,
            IUserService userService, IQueueService queueService, IEmailService emailService,
            IDocumentConverter documentConverter, IMessageService messageService, IMessenger messenger = null)
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
            _messageService = messageService;

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
            _takeTicketCommand = new AsyncRelayCommand(ExecuteTakeTicketAsync, CanTakeTicket);
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
            }
            else
            {
                // Останавливаем таймеры при отсутствии выбранной заявки
                _responseTimer.Stop();
                _resolutionTimer.Stop();

                // Сброс значений при отсутствии выбранной заявки
                StartedAtTimeValue = " ";
                CompletedAtTimeValue = " ";

                // Обновляем состояние команды
                _takeTicketCommand?.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanChangeStatus));
            }
        }

        //private string FormatTimeSpan(TimeSpan timeSpan)
        //{
        //    // Форматируем TimeSpan чтобы показывать дни, часы и минуты (без секунд)
        //    int days = timeSpan.Days;
        //    int hours = timeSpan.Hours;
        //    int minutes = timeSpan.Minutes;

        //    if (days > 0)
        //    {
        //        return $"{days} д {hours} ч {minutes} мин";
        //    }
        //    else if (hours > 0)
        //    {
        //        return $"{hours} ч {minutes} мин";
        //    }
        //    else
        //    {
        //        return $"{minutes} мин";
        //    }
        //}

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
                await _ticketService.AddTransactionAsync(new Transaction
                {
                    TicketId = SelectedTicket.Id,
                    UserId = _authService.CurrentUser.Id,
                    ActionType = "Take",
                    Details = "Заявка взята в работу",
                    Created = DateTime.Now
                });

                _messageService.ShowInfo($"Заявка {SelectedTicket.TicketNumber} взята вами на выполнение");

                // Останавливаем таймер ответа и запускаем таймер решения
                _responseTimer.Stop();
                if (SelectedTicket.CompletedAt == null)
                {
                    _resolutionTimer.Start();
                }

                // Обновляем состояние команды
                _takeTicketCommand.NotifyCanExecuteChanged();

                // Перегружаем список заявок из базы
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
                await _ticketService.AddTransactionAsync(new Transaction
                {
                    TicketId = SelectedTicket.Id,
                    UserId = _authService.CurrentUser?.Id,
                    ActionType = "Close",
                    Details = "Закрытие заявки",
                    Created = DateTime.Now
                });

                SelectedTicket.Status = TicketStatus.Closed;
                SelectedTicket.CompletedAt = DateTime.Now;

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _messageService.ShowInfo($"Заявка {SelectedTicket.TicketNumber} закрыта");

                // Останавливаем таймер решения
                _resolutionTimer.Stop();

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
                if (!_messageService.ShowConfirmation("Вы уверены, что хотите отказаться от заявки {SelectedTicket.TicketNumber}?", "Подтверждение"))
                    return;

                // Сохраняем информацию об отказе от выполнения заявки
                await _ticketService.AddTransactionAsync(new Transaction
                {
                    TicketId = SelectedTicket.Id,
                    UserId = _authService.CurrentUser?.Id,
                    ActionType = "Unassign",
                    Details = "Отказ от выполнения заявки",
                    Created = DateTime.Now
                });

                SelectedTicket.Status = TicketStatus.Active;
                SelectedTicket.SupporterId = null;
                SelectedTicket.StartedAt = null;

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _messageService.ShowInfo($"Отказ от заявки {SelectedTicket.TicketNumber} оформлен");

                // Останавливаем таймер решения и запускаем таймер ответа
                _resolutionTimer.Stop();
                _responseTimer.Start();

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
                await _ticketService.AddTransactionAsync(new Transaction
                {
                    TicketId = SelectedTicket.Id,
                    UserId = _authService.CurrentUser?.Id,
                    ActionType = "Reopen",
                    Details = "Повторное открытие заявки",
                    Created = DateTime.Now
                });

                SelectedTicket.Status = TicketStatus.Active;
                SelectedTicket.SupporterId = null;
                SelectedTicket.StartedAt = null;
                SelectedTicket.CompletedAt = null;

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _messageService.ShowInfo($"Заявка {SelectedTicket.TicketNumber} заново открыта");

                // Запускаем таймер ответа
                _responseTimer.Start();

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
                _messageService.ShowInfo($"Статус заявки {SelectedTicket.TicketNumber} изменен на {SelectedTicket.Status}");

                await LoadTickets();
                OnPropertyChanged(nameof(FilteredTickets));
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка при изменении статуса: {ex.Message}");
            }
        }

        // Команда для добавления комментария
        [RelayCommand]
        private void AddComment()
        {
            if (SelectedTicket == null) return;

            // Здесь можно реализовать диалог для добавления комментария
            _messageService.ShowInfo($"Добавление комментария к заявке {SelectedTicket.TicketNumber}");
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

        //private async void LoadTickets()
        //{
        //    var currentUser = _authService.CurrentUser;

        //    // Для "Мои тикеты"
        //    var myTickets = await _ticketService.GetTicketsByAssignee(currentUser.Id);
        //    MyTickets.Clear();
        //    foreach (var ticket in myTickets)
        //    {
        //        var requestor = await _userService.GetUserByIdAsync(ticket.RequestorId);
        //        ticket.Requestor = requestor;
        //        MyTickets.Add(ticket);
        //    }

        //    // Для "Все активные тикеты"
        //    var activeTickets = await _ticketService.GetAllActiveTickets();
        //    AllActiveTickets.Clear();
        //    foreach (var ticket in activeTickets)
        //    {
        //        var requestor = await _userService.GetUserByIdAsync(ticket.RequestorId);
        //        ticket.Requestor = requestor;
        //        AllActiveTickets.Add(ticket);
        //    }
        //}

        //[RelayCommand]
        //private async Task CloseTicket(Ticket ticket)
        //{
        //    if (ticket == null) return;
        //    await _ticketService.CloseTicket(ticket);
        //    LoadTickets();
        //}

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