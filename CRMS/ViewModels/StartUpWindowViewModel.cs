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
        [ObservableProperty]
        private DateTime currentDate = DateTime.Now;

        [ObservableProperty]
        private string currentTime = DateTime.Now.ToString("HH:mm:ss"); // Вторые часы
        private readonly IServiceProvider _serviceProvider;

        //private readonly App _app;

        [ObservableProperty]
        private bool isDarkTheme;

        public ICommand ToggleThemeCommand { get; }

        public StartUpWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // Таймер для обновления времени
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                CurrentDate = DateTime.Now;
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
