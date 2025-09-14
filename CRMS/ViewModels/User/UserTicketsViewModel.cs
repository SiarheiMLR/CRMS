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
using MaterialDesignThemes.Wpf;
using System.Windows.Input;

namespace CRMS.ViewModels.UserVM
{
    public partial class UserTicketsViewModel : ObservableObject
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

        // Коллекция для временного хранения вложений перед отправкой
        public ObservableCollection<Attachment> Attachments { get; } = new();

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
        private FlowDocument _bodyDocument = new FlowDocument()
        {
            LineHeight = 14 // Размер межстрочного интервала
        };

        [ObservableProperty]
        private string _attachmentsSummary = "Общий размер: 0 MB из 150 MB";

        private const long MaxTotalSize = 150 * 1024 * 1024; // 150 MB

        [ObservableProperty]
        private Brush _attachmentsSummaryColor = Brushes.White;

        [ObservableProperty]
        private Ticket _selectedTicket;

        private TicketPriority? _selectedPriority = null;
        public TicketPriority? SelectedPriority
        {
            get => _selectedPriority;
            set => SetProperty(ref _selectedPriority, value);
        }

        // Конструктор класса
        public UserTicketsViewModel(ITicketService ticketService, IAuthService authService,
            IUserService userService, IQueueService queueService, IEmailService emailService, IDocumentConverter documentConverter,
                            IMessenger messenger = null)
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

            CurrentUser = _authService.CurrentUser;

            // Инициализация BodyDocument с пустым параграфом и одинарным интервалом
            _bodyDocument = new FlowDocument(new Paragraph())
            {
                LineHeight = 14 // Одинарный интервал
            };

            ShowFullContentCommand = new RelayCommand<Ticket>(ShowFullContent);            

            // Запускаем загрузку асинхронно
            _ = InitializeAsync();

            _messenger = messenger;

            // Подписка на сообщения об обновлении тикетов
            if (_messenger != null)
            {
                _messenger.Register<TicketUpdatedMessage>(this, (recipient, message) =>
                {
                    // Обновляем список тикетов при получении сообщения
                    _ = LoadTicketsAsync();
                });
            }

            _messenger.Register<CommentAddedMessage>(this, (recipient, message) =>
            {
                // Обновляем комментарии для соответствующего тикета
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var ticket = Tickets.FirstOrDefault(t => t.Id == message.TicketId);
                    if (ticket != null)
                    {
                        ticket.Comments.Add(message.Comment);
                        OnPropertyChanged(nameof(ticket.Comments));

                        // Если диалог открыт для этого тикета, обновляем его
                        if (SelectedTicket?.Id == message.TicketId)
                        {
                            SelectedTicket.Comments.Add(message.Comment);
                            OnPropertyChanged(nameof(SelectedTicket));
                        }
                    }
                });
            });
        }

        // Вынесем инициализацию в отдельный метод
        private async Task InitializeAsync()
        {
            await LoadQueuesForCurrentUserAsync();
            await LoadTicketsAsync();
        }

        public class TicketUpdatedMessage { }

        private void ShowFullContent(Ticket ticket)
        {
            var dialog = new FullContentDialog
            {
                DataContext = ticket
            };

            // Проверяем, можно ли использовать главное окно как Owner
            if (Application.Current.MainWindow != null &&
                Application.Current.MainWindow.IsVisible)
            {
                dialog.Owner = Application.Current.MainWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            dialog.ShowDialog();
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
                    .Include(t => t.Attachments)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.User) // Убедитесь, что загружаются пользователи
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

        // Конвертер для FlowDocument
        private class FlowDocumentConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is FlowDocument doc) return doc;
                return new FlowDocument();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
        }

        [RelayCommand]
        private async Task CreateNewTicket()
        {            
            try
            {
                // 1. Проверки заполнения полей заявки
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

                // Новая проверка типа приоритета
                if (SelectedPriority.HasValue && SelectedPriority.Value.GetType() != typeof(TicketPriority))
                {
                    MessageBox.Show("Некорректный тип приоритета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Subject))
                {
                    MessageBox.Show("Пожалуйста, введите тему вашей заявки.", "Пустая тема заявки",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка на пустое тело заявки
                string bodyText = new TextRange(BodyDocument.ContentStart, BodyDocument.ContentEnd).Text;
                if (string.IsNullOrWhiteSpace(bodyText))
                {
                    MessageBox.Show("Пожалуйста, заполните содержание заявки.", "Пустое содержание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔥 ВАЖНО: нормализуем изображения до сохранения чтобы все картинки в FlowDocument были inline base64
                Ticket.NormalizeImagesInFlowDocument(BodyDocument);

                // 2. Создание заявки
                var newTicket = new Ticket
                {
                    Status = TicketStatus.Active,
                    Created = DateTime.Now,
                    //LastUpdated = local,               
                    QueueId = SelectedQueue.Id,        // <-- используем выбранную очередь                
                    RequestorId = CurrentUser.Id,
                    Requestor = CurrentUser,
                    Priority = SelectedPriority.Value, // т.к. nullable
                    SupporterId = null,
                    Subject = this.Subject,           // Сохраняем тему
                    ContentDocument = BodyDocument // Используем уже нормализованный документ                
                };

                // 3. Добавляем вложения перед сохранением
                foreach (var attachment in Attachments)
                {
                    newTicket.Attachments.Add(new Attachment
                    {
                        FileName = attachment.FileName,
                        ContentType = attachment.ContentType,
                        FileData = attachment.FileData,
                        FileSize = attachment.FileData.Length,
                        UploadedById = CurrentUser.Id
                    });
                }

                // 4. Сохраняем тикет (async/await строго последовательно)
                await _ticketService.AddTicketAsync(newTicket);
                                
                // 5. Добавляем в коллекции
                Tickets.Add(newTicket);
                OpenTickets.Add(newTicket);

                // 6. Отправляем сообщение другим VM об обновлении
                _messenger?.Send(new TicketUpdatedMessage());

                // 7. Отправка email
                // 7.1. Формируем HTML для писем
                string ticketBodyHtml = _documentConverter.FlowDocumentToHtml(newTicket.ContentDocument);

                // 7.2. Формируем список вложений в HTML
                string attachmentsHtml;
                if (newTicket.Attachments.Any())
                {
                    attachmentsHtml = string.Join("", newTicket.Attachments.Select(a =>
                        $"<li>{a.FileName} ({GetFileSize(a.FileData.Length)})</li>"));
                }
                else
                {
                    attachmentsHtml = "<li class='no-attachments'>Отсутствуют</li>";
                }

                // 7.3. Параметры
                var baseParams = new Dictionary<string, string>
                {
                    { "UserName", newTicket.Requestor.DisplayName ?? newTicket.Requestor.Email },
                    { "UserEmail", newTicket.Requestor.Email },
                    { "Subject", newTicket.Subject },
                    { "Queue", SelectedQueue.Name },
                    { "Priority", GetPriorityText(newTicket.Priority) }, // <-- перевод в русский
                    { "PriorityColor", GetPriorityColor(newTicket.Priority) }, // <-- цвет
                    { "Created", newTicket.Created.ToString("g") },
                    { "TicketNumber", newTicket.TicketNumber }
                };                               

                // 7.4. Генерируем письмо для пользователя
                string userBody = Templates.TicketCreated(ticketBodyHtml, attachmentsHtml);
                foreach (var p in baseParams)
                    userBody = userBody.Replace('{' + p.Key + '}', p.Value);               

                // Отправляем пользователю
                await _emailService.SendEmailWithAttachmentsAsync(CurrentUser.Email,
                    $"Ваша заявка с номером {newTicket.TicketNumber} зарегистрирована в системе CRMS!", userBody, newTicket.Attachments);

                // 7.5. Генерируем письмо поддержке
                string supportBody = Templates.TicketCreatedForSupport(ticketBodyHtml, attachmentsHtml);
                foreach (var p in baseParams)
                    supportBody = supportBody.Replace('{' + p.Key + '}', p.Value);
                Debug.WriteLine(supportBody);

                // Отправляем поддержке
                await _emailService.SendEmailWithAttachmentsAsync(SelectedQueue.CorrespondAddress,
                    $"В системе CRMS зарегистрирована новая заявка с номером {newTicket.TicketNumber}!", supportBody, newTicket.Attachments);

                // 8. Сброс формы                
                ResetForm();

                MessageBox.Show("Заявка успешно создана!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заявки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }  
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

        private string GetPriorityColor(TicketPriority priority)
        {
            return priority switch
            {
                TicketPriority.Low => "#008000",     // зелёный
                TicketPriority.Mid => "#FFA500",  // оранжевый
                TicketPriority.High => "#FF0000",    // красный
                _ => "#000000"                       // чёрный по умолчанию
            };
        }


        private void ResetForm()
        {
            SelectedQueue = null;
            SelectedPriority = null;
            Subject = string.Empty;
            // Сбрасываем документ с правильной инициализацией
            BodyDocument = new FlowDocument(new Paragraph())
            {
                LineHeight = 14 // Одинарный интервал
            };
            Attachments.Clear();

            // Обновляем строку с общим размером
            UpdateAttachmentsSummary();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadTicketsAsync();
        }

        // Метод для добавления файлов
        public async Task AddFilesAsync(string[] filePaths)
        {
            // Белый список расширений
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx",
                ".png", ".jpg", ".jpeg", ".gif", ".txt", ".rtf", ".zip"
            };

            const long maxFileSize = 30 * 1024 * 1024; // 30 MB
            const long maxTotalSize = 150 * 1024 * 1024;  // 150 MB на все вложения

            foreach (var filePath in filePaths)
            {
                var ext = Path.GetExtension(filePath);
                if (!allowedExtensions.Contains(ext))
                {
                    MessageBox.Show($"Файл {Path.GetFileName(filePath)} имеет недопустимый тип.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                var fileInfo = new FileInfo(filePath);

                // Проверка размера одного файла
                if (fileInfo.Length > maxFileSize)
                {
                    MessageBox.Show(
                        $"Файл {fileInfo.Name} превышает допустимый размер ({maxFileSize / 1024 / 1024} MB).",
                        "Слишком большой файл",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    continue;
                }

                // Проверка суммарного размера
                long currentTotal = Attachments.Sum(a => a.FileData?.Length ?? 0);
                if (currentTotal + fileInfo.Length > maxTotalSize)
                {
                    MessageBox.Show(
                        $"Нельзя добавить {fileInfo.Name}: суммарный размер вложений превысит {maxTotalSize / 1024 / 1024} MB.",
                        "Превышение общего лимита",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    continue;
                }

                var bytes = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);

                // Проверка на дубликат по имени и размеру
                bool duplicate = Attachments.Any(a =>
                    string.Equals(a.FileName, fileName, StringComparison.OrdinalIgnoreCase) &&
                    a.FileData != null &&
                    a.FileData.Length == bytes.Length);

                if (duplicate)
                {
                    MessageBox.Show(
                        $"Файл {fileName} уже был добавлен ранее.",
                        "Дубликат вложения",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    continue;
                }

                using var stream = File.OpenRead(filePath);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream); // Потоковое чтение

                var attachment = new Attachment
                {
                    FileName = Path.GetFileName(filePath),
                    ContentType = GetMimeType(ext),
                    FileData = memoryStream.ToArray(),
                    FileSize = memoryStream.Length,
                    UploadedById = CurrentUser.Id
                };

                Attachments.Add(attachment);
            }

            UpdateAttachmentsSummary();
        }

        // Метод для получения MIME-типа (простейший словарь)
        private string GetMimeType(string ext) => ext.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".rtf" => "application/rtf",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };

        [RelayCommand]
        private void Save()
        {
            // Проверяем, есть ли текст в редакторе
            string documentText = new TextRange(BodyDocument.ContentStart, BodyDocument.ContentEnd).Text;
            if (string.IsNullOrWhiteSpace(documentText))
            {
                MessageBox.Show("Редактор не содержит текста. Нечего сохранять.",
                                "Пустой документ",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
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
                    var exportDoc = PrepareDocumentForExport(BodyDocument);

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

                    MessageBox.Show("Документ успешно сохранён!", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
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


        // Метод для сохранения FlowDocument в PDF через XPS
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

        // Конвертация XPS → PDF через PdfSharp: рендерим каждую FixedPage в PNG и кладём как изображение
        private void ConvertXpsToPdf(string xpsPath, string pdfPath)
        {
            using var xpsDoc = new XpsDocument(xpsPath, FileAccess.Read);
            var fixedDocSeq = xpsDoc.GetFixedDocumentSequence();

            using var pdfDoc = new PdfSharp.Pdf.PdfDocument();

            const double dpi = 200.0;            // качество рендера (можно поднять до 200–300)
            const double ptPerDip = 72.0 / 96.0; // перевод единиц WPF (DIP) в пункты PDF

            foreach (var docRef in fixedDocSeq.References)
            {
                var fixedDoc = docRef.GetDocument(false);
                foreach (PageContent pageContent in fixedDoc.Pages)
                {
                    // Было: pageRef.GetPage(); —> Нужен GetPageRoot(false)
                    var fixedPage = pageContent.GetPageRoot(false) as FixedPage;
                    if (fixedPage == null) continue;

                    // Убедимся, что страница измерена/размещена
                    fixedPage.Measure(new Size(fixedPage.Width, fixedPage.Height));
                    fixedPage.Arrange(new Rect(new Size(fixedPage.Width, fixedPage.Height)));
                    fixedPage.UpdateLayout();

                    // Создаём PDF-страницу с правильными размерами (в пунктах)
                    var pdfPage = pdfDoc.AddPage();
                    pdfPage.Width = fixedPage.Width * ptPerDip;
                    pdfPage.Height = fixedPage.Height * ptPerDip;

                    // Рендерим FixedPage в PNG во временный файл
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

                        // Рисуем PNG на PDF-странице
                        using (var gfx = XGraphics.FromPdfPage(pdfPage))
                        using (var img = XImage.FromFile(tmpPng))
                        {
                            gfx.DrawImage(img, 0, 0, pdfPage.Width, pdfPage.Height);
                        }
                    }
                    finally
                    {
                        try { if (File.Exists(tmpPng)) File.Delete(tmpPng); } catch { /* ignore */ }
                    }
                }
            }

            pdfDoc.Save(pdfPath);
        }

        [RelayCommand]
        private void RemoveAttachment(Attachment attachment)
        {
            if (attachment != null && Attachments.Contains(attachment))
                Attachments.Remove(attachment);

            UpdateAttachmentsSummary();
        }

        private void UpdateAttachmentsSummary()
        {
            long totalSize = Attachments.Sum(a => a.FileData?.Length ?? 0);
            double sizeInMb = totalSize / (1024.0 * 1024.0);
            double maxInMb = MaxTotalSize / (1024.0 * 1024.0);

            AttachmentsSummary = $"Общий размер: {sizeInMb:F1} MB из {maxInMb:F0} MB";

            // Меняем цвет при превышении
            AttachmentsSummaryColor = totalSize > MaxTotalSize ? Brushes.Red : Brushes.White;
        }

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
                    MessageBox.Show("Файл успешно сохранен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task ShowComments(Ticket ticket)
        {
            if (ticket == null || ticket.Comments == null || ticket.Comments.Count == 0)
                return;

            SelectedTicket = ticket;

            var view = new ViewCommentDialog
            {
                DataContext = this   // 👈 передаём текущий VM
            };

            await DialogHost.Show(view, "CommentsDialogHost");
        }

        // Вспомогательный проверочный метод (чтобы не забыть _authService в коде)
        private void _auth_service_check(IAuthService auth) { /* no-op, просто для соблюдения порядка параметров */ }
    }
}