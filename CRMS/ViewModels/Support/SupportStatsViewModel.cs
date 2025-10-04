using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CRMS.ViewModels.Support
{
    public partial class SupportStatsViewModel : ObservableObject
    {
        // Добавляем свойства для отображения статусов и приоритетов
        [ObservableProperty]
        private ObservableCollection<string> statusLabelsRussian = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<SKColor> statusColors = new ObservableCollection<SKColor>();

        [ObservableProperty]
        private ObservableCollection<string> priorityLabelsRussian = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<SKColor> priorityColors = new ObservableCollection<SKColor>();

        // Типы периодов
        [ObservableProperty]
        private string _selectedPeriodType = "Неделя";

        public List<string> PeriodTypes { get; } = new() { "Неделя", "Месяц", "Квартал", "Год" };

        // Доступные периоды для выбора
        [ObservableProperty]
        private ObservableCollection<string> _availablePeriods = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedPeriod;

        // Годы для выбора
        [ObservableProperty]
        private ObservableCollection<int> _availableYears = new ObservableCollection<int>();

        [ObservableProperty]
        private int _selectedYear = DateTime.Now.Year;

        // ===== 1. Карточки ключевых метрик =====
        [ObservableProperty] private int totalTickets = 152;
        [ObservableProperty] private int activeTickets = 45;
        [ObservableProperty] private int inProgressTickets = 27;
        [ObservableProperty] private int closedTickets = 80;
        [ObservableProperty] private double avgResolutionTime = 5.3; // часы
        [ObservableProperty] private double slaSuccessRate = 87.5; // %

        // ===== 2. Распределение по приоритетам =====
        [ObservableProperty]
        private ObservableCollection<ISeries> prioritySeries = new ObservableCollection<ISeries>();

        // ===== 3. Диаграмма по статусам =====
        [ObservableProperty]
        private ObservableCollection<ISeries> statusSeries = new ObservableCollection<ISeries>();

        [ObservableProperty]
        private string[] statusLabels;

        [ObservableProperty]
        private Axis[] statusXAxes;

        [ObservableProperty]
        private Axis[] statusYAxes;

        // ===== 4. Динамика по времени =====
        [ObservableProperty]
        private ObservableCollection<ISeries> timelineSeries = new ObservableCollection<ISeries>();

        [ObservableProperty]
        private string[] timelineLabels;

        [ObservableProperty]
        private Axis[] timelineXAxes;

        [ObservableProperty]
        private Axis[] timelineYAxes;

        // ===== 5. Время отклика =====
        [ObservableProperty]
        private ObservableCollection<ISeries> responseTimeSeries = new ObservableCollection<ISeries>();

        [ObservableProperty]
        private string[] responseTimeLabels;

        [ObservableProperty]
        private Axis[] responseTimeXAxes;

        [ObservableProperty]
        private Axis[] responseTimeYAxes;

        // ===== 6. SLA =====
        [ObservableProperty]
        private ObservableCollection<ISeries> slaSeries = new ObservableCollection<ISeries>();

        // ===== 7. ТОП очереди =====
        [ObservableProperty]
        private ObservableCollection<ISeries> queueSeries = new ObservableCollection<ISeries>();

        [ObservableProperty]
        private string[] queueLabels;

        [ObservableProperty]
        private Axis[] queueXAxes;

        [ObservableProperty]
        private Axis[] queueYAxes;

        // ===== 8. Перераспределения / возвраты =====
        [ObservableProperty]
        private ObservableCollection<ISeries> reassignSeries = new ObservableCollection<ISeries>();

        [ObservableProperty]
        private string[] reassignLabels;

        [ObservableProperty]
        private Axis[] reassignXAxes;

        [ObservableProperty]
        private Axis[] reassignYAxes;

        // ===== 9. Комментарии =====
        [ObservableProperty]
        private ObservableCollection<ISeries> commentsSeries = new ObservableCollection<ISeries>();

        [ObservableProperty]
        private string[] commentsLabels;

        [ObservableProperty]
        private Axis[] commentsXAxes;

        [ObservableProperty]
        private Axis[] commentsYAxes;

        // ===== 10. Воронка =====
        [ObservableProperty]
        private ObservableCollection<ISeries> funnelSeries = new ObservableCollection<ISeries>();

        [ObservableProperty]
        private string[] funnelLabels;

        [ObservableProperty]
        private Axis[] funnelXAxes;

        [ObservableProperty]
        private Axis[] funnelYAxes;

        public SupportStatsViewModel()
        {
            // Инициализируем годы (последние 5 лет + текущий)
            for (int year = DateTime.Now.Year - 5; year <= DateTime.Now.Year; year++)
            {
                AvailableYears.Add(year);
            }

            // Обновляем доступные периоды
            UpdateAvailablePeriods();

            // Загружаем данные
            LoadMockData();
        }

        partial void OnSelectedPeriodTypeChanged(string value)
        {
            UpdateAvailablePeriods();
            LoadMockData();
        }

        partial void OnSelectedPeriodChanged(string value)
        {
            LoadMockData();
        }

        partial void OnSelectedYearChanged(int value)
        {
            UpdateAvailablePeriods();
            LoadMockData();
        }

        private void UpdateAvailablePeriods()
        {
            AvailablePeriods.Clear();

            switch (SelectedPeriodType)
            {
                case "Неделя":
                    // Генерируем недели для выбранного года
                    var startDate = new DateTime(SelectedYear, 1, 1);
                    while (startDate.Year == SelectedYear)
                    {
                        var endDate = startDate.AddDays(6);
                        if (endDate.Year != SelectedYear)
                            endDate = new DateTime(SelectedYear, 12, 31);

                        AvailablePeriods.Add($"{startDate:dd.MM} - {endDate:dd.MM}");
                        startDate = startDate.AddDays(7);
                    }
                    SelectedPeriod = AvailablePeriods.FirstOrDefault();
                    break;

                case "Месяц":
                    // Месяцы года
                    for (int month = 1; month <= 12; month++)
                    {
                        AvailablePeriods.Add(new DateTime(SelectedYear, month, 1).ToString("MMMM"));
                    }
                    SelectedPeriod = new DateTime(SelectedYear, DateTime.Now.Month, 1).ToString("MMMM");
                    break;

                case "Квартал":
                    // Кварталы года
                    AvailablePeriods.Add("1 квартал");
                    AvailablePeriods.Add("2 квартал");
                    AvailablePeriods.Add("3 квартал");
                    AvailablePeriods.Add("4 квартал");

                    // Определяем текущий квартал
                    int currentQuarter = (DateTime.Now.Month - 1) / 3 + 1;
                    SelectedPeriod = $"{currentQuarter} квартал";
                    break;

                case "Год":
                    // Просто показываем выбранный год
                    AvailablePeriods.Add(SelectedYear.ToString());
                    SelectedPeriod = SelectedYear.ToString();
                    break;
            }
        }

        private void LoadMockData()
        {
            // Очистка коллекций перед загрузкой новых данных
            PrioritySeries.Clear();
            StatusSeries.Clear();
            TimelineSeries.Clear();
            ResponseTimeSeries.Clear();
            SlaSeries.Clear();
            QueueSeries.Clear();
            ReassignSeries.Clear();
            CommentsSeries.Clear();
            FunnelSeries.Clear();

            // Загружаем данные в зависимости от выбранного периода
            switch (SelectedPeriodType)
            {
                case "Неделя":
                    LoadWeekStats();
                    break;
                case "Месяц":
                    LoadMonthStats();
                    break;
                case "Квартал":
                    LoadQuarterStats();
                    break;
                case "Год":
                    LoadYearStats();
                    break;
            }

            // Остальные данные, которые не зависят от периода
            LoadCommonStats();
        }

        private void LoadCommonStats()
        {
            // --- приоритеты ---
            PrioritySeries.Add(new PieSeries<double>
            {
                Values = new double[] { 40 },
                Name = "Низкий",
                Fill = new SolidColorPaint(SKColors.Green)
            });
            PrioritySeries.Add(new PieSeries<double>
            {
                Values = new double[] { 70 },
                Name = "Средний",
                Fill = new SolidColorPaint(SKColors.Orange)
            });
            PrioritySeries.Add(new PieSeries<double>
            {
                Values = new double[] { 42 },
                Name = "Высокий",
                Fill = new SolidColorPaint(SKColors.Red)
            });

            // --- статусы ---
            StatusLabels = new[] { "Active", "InProgress", "Closed" };
            StatusSeries.Add(new ColumnSeries<int>
            {
                Values = new[] { 45, 27, 80 },
                Name = "Tickets",
                Fill = new SolidColorPaint(SKColors.CornflowerBlue)
            });
            StatusXAxes = new[] { new Axis { Labels = StatusLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            StatusYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // --- время отклика ---
            ResponseTimeLabels = new[] { "<1ч", "1-4ч", "4-24ч", ">24ч" };
            ResponseTimeSeries.Add(new ColumnSeries<int>
            {
                Values = new[] { 15, 30, 20, 5 },
                Name = "Ответы",
                Fill = new SolidColorPaint(SKColors.Yellow)
            });
            ResponseTimeXAxes = new[] { new Axis { Labels = ResponseTimeLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            ResponseTimeYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // --- SLA ---
            SlaSeries.Add(new PieSeries<double>
            {
                Values = new double[] { 87.5 },
                Name = "Успели",
                Fill = new SolidColorPaint(SKColors.Green)
            });
            SlaSeries.Add(new PieSeries<double>
            {
                Values = new double[] { 12.5 },
                Name = "Не успели",
                Fill = new SolidColorPaint(SKColors.Red)
            });

            // --- ТОП очереди ---
            QueueLabels = new[] { "IT", "HR", "Finance" };
            QueueSeries.Add(new ColumnSeries<int>
            {
                Values = new[] { 60, 40, 52 },
                Name = "Заявки",
                Fill = new SolidColorPaint(SKColors.Purple)
            });
            QueueXAxes = new[] { new Axis { Labels = QueueLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            QueueYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // --- перераспределения ---
            ReassignLabels = new[] { "Unassign", "Reopen" };
            ReassignSeries.Add(new ColumnSeries<int>
            {
                Values = new[] { 12, 8 },
                Name = "События",
                Fill = new SolidColorPaint(SKColors.OrangeRed)
            });
            ReassignXAxes = new[] { new Axis { Labels = ReassignLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            ReassignYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // --- комментарии ---
            CommentsLabels = new[] { "В среднем" };
            CommentsSeries.Add(new ColumnSeries<double>
            {
                Values = new[] { 2.3 },
                Name = "Комментарии",
                Fill = new SolidColorPaint(SKColors.Cyan)
            });
            CommentsXAxes = new[] { new Axis { Labels = CommentsLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            CommentsYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // --- воронка ---
            FunnelLabels = new[] { "Открытые", "В работе", "Решённые", "Закрытые" };
            FunnelSeries.Add(new ColumnSeries<int>
            {
                Values = new[] { 152, 100, 90, 80 },
                Name = "Воронка",
                Fill = new SolidColorPaint(SKColors.SkyBlue)
            });
            FunnelXAxes = new[] { new Axis { Labels = FunnelLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            FunnelYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };
        }

        private void LoadWeekStats()
        {
            // Генерируем данные для выбранной недели
            int weekIndex = AvailablePeriods.IndexOf(SelectedPeriod);

            TimelineLabels = new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

            TimelineSeries.Clear();
            TimelineSeries.Add(new LineSeries<int>
            {
                Values = new[] { 5, 10, 8, 12, 15, 7, 3 },
                Name = "Заявки",
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LightBlue, 2)
            });

            TimelineXAxes = new[] { new Axis { Labels = TimelineLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            TimelineYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // Обновляем данные карточек для недели
            TotalTickets = 45;
            ActiveTickets = 15;
            InProgressTickets = 20;
            ClosedTickets = 10;
            AvgResolutionTime = 3.2;
            SlaSuccessRate = 92.5;
        }

        private void LoadMonthStats()
        {
            // Генерируем данные для выбранного месяца
            int daysInMonth = DateTime.DaysInMonth(SelectedYear, DateTime.ParseExact(SelectedPeriod, "MMMM", null).Month);

            TimelineLabels = Enumerable.Range(1, daysInMonth).Select(d => d.ToString()).ToArray();

            TimelineSeries.Clear();
            TimelineSeries.Add(new LineSeries<int>
            {
                Values = Enumerable.Range(1, daysInMonth).Select(_ => Random.Shared.Next(1, 20)).ToArray(),
                Name = "Заявки",
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LightGreen, 2)
            });

            TimelineXAxes = new[] { new Axis { Labels = TimelineLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            TimelineYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // Обновляем данные карточек для месяца
            TotalTickets = 250;
            ActiveTickets = 75;
            InProgressTickets = 100;
            ClosedTickets = 75;
            AvgResolutionTime = 4.8;
            SlaSuccessRate = 88.3;
        }

        private void LoadQuarterStats()
        {
            // Генерируем данные для выбранного квартала
            int quarter = int.Parse(SelectedPeriod.Split(' ')[0]);
            string[] monthsInQuarter = quarter switch
            {
                1 => new[] { "Январь", "Февраль", "Март" },
                2 => new[] { "Апрель", "Май", "Июнь" },
                3 => new[] { "Июль", "Август", "Сентябрь" },
                4 => new[] { "Октябрь", "Ноябрь", "Декабрь" },
                _ => new[] { "Январь", "Февраль", "Март" }
            };

            TimelineLabels = monthsInQuarter;

            TimelineSeries.Clear();
            TimelineSeries.Add(new ColumnSeries<int>
            {
                Values = new[] { 150, 200, 180 },
                Name = "Заявки",
                Fill = new SolidColorPaint(SKColors.Coral)
            });

            TimelineXAxes = new[] { new Axis { Labels = TimelineLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            TimelineYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // Обновляем данные карточек для квартала
            TotalTickets = 530;
            ActiveTickets = 160;
            InProgressTickets = 220;
            ClosedTickets = 150;
            AvgResolutionTime = 5.6;
            SlaSuccessRate = 85.7;
        }

        private void LoadYearStats()
        {
            // Генерируем данные для выбранного года
            TimelineLabels = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

            TimelineSeries.Clear();
            TimelineSeries.Add(new ColumnSeries<int>
            {
                Values = new[] { 50, 60, 70, 65, 80, 90, 100, 95, 85, 75, 65, 55 },
                Name = "Заявки",
                Fill = new SolidColorPaint(SKColors.Blue)
            });

            TimelineXAxes = new[] { new Axis { Labels = TimelineLabels, LabelsPaint = new SolidColorPaint(SKColors.White) } };
            TimelineYAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(SKColors.White) } };

            // Обновляем данные карточек для года
            TotalTickets = 1850;
            ActiveTickets = 550;
            InProgressTickets = 750;
            ClosedTickets = 550;
            AvgResolutionTime = 6.2;
            SlaSuccessRate = 82.4;
        }
    }
}