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
using System.Windows.Media.Imaging;                                     // RenderTargetBitmap, PngBitmapEncoder

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

        private TicketPriority? _selectedPriority = null;
        public TicketPriority? SelectedPriority
        {
            get => _selectedPriority;
            set => SetProperty(ref _selectedPriority, value);
        }

        // Конструктор класса
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

            // Инициализация BodyDocument с пустым параграфом и одинарным интервалом
            _bodyDocument = new FlowDocument(new Paragraph())
            {
                LineHeight = 14 // Одинарный интервал
            };

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
                //LastUpdated = local,               
                QueueId = SelectedQueue.Id,        // <-- используем выбранную очередь
                RequestorId = CurrentUser.Id,
                Priority = SelectedPriority.Value, // т.к. nullable
                SupporterId = null,
                Subject = this.Subject,           // Сохраняем тему
                ContentDocument = BodyDocument // Используем текущий документ
            };

            // Устанавливаем документ ПОСЛЕ создания объекта
            //newTicket.ContentDocument = BodyDocument;

            // Сохраняем вложения при создании тикета
            newTicket.Attachments = Attachments.ToList();

            try
            {
                await _ticketService.AddTicketAsync(newTicket);

                // Добавляем в коллекции
                Tickets.Add(newTicket);
                OpenTickets.Add(newTicket);

                // Сбрасываем форму                
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

        // Метод для добавления файлов
        public void AddFiles(string[] filePaths)
        {
            // Белый список расширений
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx",
                ".png", ".jpg", ".jpeg", ".gif", ".txt", ".rtf", ".zip"
            };

            foreach (var filePath in filePaths)
            {
                var ext = Path.GetExtension(filePath);
                if (!allowedExtensions.Contains(ext))
                {
                    MessageBox.Show($"Файл {Path.GetFileName(filePath)} имеет недопустимый тип.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                var bytes = File.ReadAllBytes(filePath);

                var attachment = new Attachment
                {
                    FileName = Path.GetFileName(filePath),
                    ContentType = GetMimeType(ext),
                    FileData = bytes
                };

                Attachments.Add(attachment);
            }
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

        // Конвертация XPS → PDF через PdfSharp
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
        }


        // Вспомогательный проверочный метод (чтобы не забыть _authService в коде)
        private void _auth_service_check(IAuthService auth) { /* no-op, просто для соблюдения порядка параметров */ }
    }
}