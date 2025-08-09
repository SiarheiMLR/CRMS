using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.FaqService;
using CRMS.Domain.Entities;
using CRMS.Domain.DTOs;
using CRMS.Services;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CRMS.Business.Services.AuthService;

namespace CRMS.ViewModels.Faq
{
    public partial class FaqAdminPageViewModel : ObservableObject
    {
        private readonly IFaqService _faqService;
        private readonly User _currentUser;

        public FaqAdminPageViewModel(IFaqService faqService, IAuthService authService)
        {
            _faqService = faqService;
            _currentUser = authService.CurrentUser;

            SnackQueue = new SnackbarMessageQueue();
            Categories = new ObservableCollection<FaqCategory>();
            PopularFaqs = new ObservableCollection<FaqItem>();
            TopAuthors = new ObservableCollection<PopularAuthorDto>();
            CategoryStats = new ObservableCollection<CategoryStatDto>();

            LoadAnalytics();
        }

        public ISnackbarMessageQueue SnackQueue { get; }

        [ObservableProperty]
        private int stepIndex = 0;

        [ObservableProperty]
        private string newQuestion;
        [ObservableProperty]
        private string newAnswer;
        [ObservableProperty]
        private string tagInput;
        [ObservableProperty]
        private FaqCategory selectedCategory;
        [ObservableProperty]
        private string newCategoryName;

        public ObservableCollection<FaqCategory> Categories { get; }
        public ObservableCollection<FaqItem> PopularFaqs { get; }
        public ObservableCollection<PopularAuthorDto> TopAuthors { get; }

        [ObservableProperty]
        private ObservableCollection<CategoryStatDto> categoryStats;

        private async void LoadAnalytics()
        {
            try
            {
                var popular = await _faqService.GetTopPopularAsync(20);
                PopularFaqs.Clear();
                foreach (var item in popular)
                    PopularFaqs.Add(item);

                var authors = await _faqService.GetTopAuthorsAsync();
                TopAuthors.Clear();
                foreach (var author in authors)
                    TopAuthors.Add(author);

                var cats = await _faqService.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var cat in cats)
                    Categories.Add(cat);

                var stats = await _faqService.GetCategoryStatsAsync();
                CategoryStats.Clear();
                foreach (var stat in stats)
                    CategoryStats.Add(stat);
            }
            catch (Exception ex)
            {
                SnackQueue.Enqueue($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task PublishAsync()
        {
            if (string.IsNullOrWhiteSpace(NewQuestion) || string.IsNullOrWhiteSpace(NewAnswer) || SelectedCategory == null)
            {
                SnackQueue.Enqueue("Заполните все поля");
                return;
            }

            try
            {
                var tags = tagInput?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .ToList() ?? new List<string>();

                await _faqService.CreateFaqAsync(NewQuestion, NewAnswer, SelectedCategory.Id, tags, _currentUser.Id);

                SnackQueue.Enqueue("FAQ успешно опубликован");

                StepIndex = 0;
                NewQuestion = NewAnswer = TagInput = string.Empty;
            }
            catch (Exception ex)
            {
                SnackQueue.Enqueue($"Ошибка публикации: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                SnackQueue.Enqueue("Название категории не может быть пустым.");
                return;
            }

            var exists = Categories.Any(c => c.Title.Trim().ToLower() == NewCategoryName.Trim().ToLower());
            if (exists)
            {
                SnackQueue.Enqueue("Такая категория уже существует.");
                return;
            }

            try
            {
                var newCategory = await _faqService.CreateCategoryAsync(NewCategoryName.Trim());
                Categories.Add(newCategory);
                NewCategoryName = string.Empty;
                SnackQueue.Enqueue("Категория успешно добавлена.");
            }
            catch (Exception ex)
            {
                SnackQueue.Enqueue($"Ошибка при добавлении категории: {ex.Message}");
            }
        }
    }
}
