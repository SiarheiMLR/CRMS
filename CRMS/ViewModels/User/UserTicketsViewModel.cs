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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Windows.Documents;
using System.IO;
using System.Text;
using CRMS.Views.User;

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

        // Коллекция приоритетов
        public ObservableCollection<TicketPriority> Priorities { get; } =
            new ObservableCollection<TicketPriority>(
                Enum.GetValues(typeof(TicketPriority)).Cast<TicketPriority>()
            );

        //[ObservableProperty]
        //private Queue _selectedQueue; // привязка SelectedItem ComboBox

        [ObservableProperty]
        private Queue? _selectedQueue = null; // Явная инициализация null в ComboBox       

        [ObservableProperty]
        private ICollectionView _groupedTickets;

        [ObservableProperty]
        private User _currentUser;

        [ObservableProperty]
        private string _subject = string.Empty;

        [ObservableProperty]
        private FlowDocument _bodyDocument = RichTextEditor.DefaultDocument;

        private TicketPriority? _selectedPriority = null;
        public TicketPriority? SelectedPriority
        {
            get => _selectedPriority;
            set => SetProperty(ref _selectedPriority, value);
        }


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

            // Инициализация по умолчанию
            //_selectedPriority = TicketPriority.Mid;

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

                // УБРАТЬ выбор первой очереди по умолчанию!
                // SelectedQueue = Queues.FirstOrDefault();
            }
            catch (Exception ex)
            {
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
                // Безопасная инициализация документа
                if (string.IsNullOrWhiteSpace(ticket.Content))
                {
                    ticket.Content = string.Empty;
                }
                else
                {
                    try
                    {
                        var _ = ticket.ContentDocument;
                    }
                    catch
                    {
                        // В случае ошибки сбрасываем содержимое
                        ticket.Content = string.Empty;
                    }
                }

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

            if (SelectedPriority is null)
            {
                MessageBox.Show("Пожалуйста, выберите приоритет.", "Не выбран приоритет",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Subject))
            {
                MessageBox.Show("Пожалуйста, введите тему вашей заявки.", "Пустая тема заявки",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Конвертируем FlowDocument в XAML
            string bodyXaml = ConvertFlowDocumentToXaml(BodyDocument);

            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Minsk"); // Windows на Linux/Mac может быть "Europe/Minsk"
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            var newTicket = new Ticket
            {
                Status = TicketStatus.Active,
                Created = local,
                LastUpdated = local,               
                QueueId = SelectedQueue.Id,        // <-- используем выбранную очередь
                RequestorId = CurrentUser.Id,
                Priority = SelectedPriority.Value, // т.к. nullable
                SupporterId = null,
                Subject = this.Subject,           // Сохраняем тему
                Content = string.Empty // Инициализируем пустой строкой
            };

            // Устанавливаем документ ПОСЛЕ создания объекта
            newTicket.ContentDocument = BodyDocument;

            try
            {
                await _ticketService.AddTicketAsync(newTicket);

                // Добавляем в коллекции
                Tickets.Add(newTicket);
                OpenTickets.Add(newTicket);

                // Сбрасываем форму
                SelectedQueue = null;
                SelectedPriority = null;
                Subject = string.Empty;
                BodyDocument = new FlowDocument();

                MessageBox.Show("Заявка успешно создана!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заявки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ConvertFlowDocumentToXaml(FlowDocument document)
        {
            if (document == null)
                return string.Empty;

            try
            {
                var range = new TextRange(document.ContentStart, document.ContentEnd);
                using (var stream = new MemoryStream())
                {
                    range.Save(stream, DataFormats.Xaml);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static FlowDocument ConvertXamlToFlowDocument(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return new FlowDocument();

            try
            {
                var document = new FlowDocument();
                var range = new TextRange(document.ContentStart, document.ContentEnd);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                {
                    range.Load(stream, DataFormats.Xaml);
                }
                return document;
            }
            catch (Exception)
            {
                // Возвращаем пустой документ в случае ошибки
                return new FlowDocument();
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