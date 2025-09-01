using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.FaqService;
using CRMS.Domain.Entities;
using CRMS.Services;
using System.Collections.ObjectModel;
using System.Windows;
using MaterialDesignThemes.Wpf;
using CRMS.Views.Faq;
using MailKit.Search;

namespace CRMS.ViewModels.Faq
{
    public partial class FaqPageViewModel : ObservableObject
    {
        private readonly IFaqService _faqService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<FaqCategory> Categories { get; } = new();
        public ISnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

        [ObservableProperty]
        private string searchQuery;

        public FaqPageViewModel(IFaqService faqService, INavigationService navigationService)
        {
            _faqService = faqService;
            _navigationService = navigationService;
            LoadCategoriesAsync();
        }

        private async void LoadCategoriesAsync()
        {
            try
            {
                var categories = await _faqService.GetAllCategoriesAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Categories.Clear();
                    foreach (var cat in categories)
                        Categories.Add(cat);
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Ошибка загрузки: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) { LoadCategoriesAsync(); return; }

            var result = await _faqService.SearchFaqsAsync(SearchQuery);
            Categories.Clear();
            Categories.Add(new FaqCategory
            {
                Title = $"Результаты по запросу «{SearchQuery}»",
                Items = result
            });

            SnackbarMessageQueue.Enqueue($"Найдено {result.Count} совпадений");
        }

        public void NavigateToDetail(FaqItem item)
        {
            _navigationService.NavigateToWithParameter<FaqDetailPage>(item);
        }        
    }
}
