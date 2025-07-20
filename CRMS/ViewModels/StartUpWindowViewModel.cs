using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Threading;
using System.Windows.Input;
using CRMS.Views; // Подключаем пространство имен с окном авторизации
using Microsoft.Extensions.DependencyInjection; // Подключаем сервис навигации


namespace CRMS.ViewModels
{
    public partial class StartUpWindowViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

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

        //private readonly App _app;

        [ObservableProperty]
        private bool isDarkTheme;

        public ICommand ToggleThemeCommand { get; }

        public StartUpWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // Таймер обновляет время каждую секунду
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                CurrentDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            };
            timer.Start();

            // Инициализация темы при старте
            //_app = (App)App.Current;
            //IsDarkTheme = _app.LoadThemeSetting();
            //ToggleThemeCommand = new RelayCommand(ToggleTheme);
        }

        // Логика открытия окна авторизации
        [RelayCommand]
        public void OpenLoginWindow()
        {
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>(); 

            loginWindow.Show();
        }

        // Этот метод будет вызываться автоматически при изменении IsDarkTheme
        //private void ToggleTheme()
        //{
        //    IsDarkTheme = !IsDarkTheme;
        //    _app.ApplyTheme(IsDarkTheme);
        //    _app.SaveThemeSetting(IsDarkTheme);
        //}        
    }
}
