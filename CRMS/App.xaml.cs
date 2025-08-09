using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Windows;
using CRMS.Services;
using CRMS.ViewModels;
using CRMS.Views;
using CRMS.DAL.Data;
using CRMS.DAL.Repositories;
using CRMS.Domain.Interfaces;
using CRMS.Business.Services.AuthService;
using Microsoft.EntityFrameworkCore;
using CRMS.Business.Services.UserService;
using CRMS.Business.Services.TicketService;
using CRMS.Business.Services.AttachmentService;
using CRMS.Views.Admin;
using CRMS.Views.Faq;
using CRMS.ViewModels.Admin;
using CRMS.ViewModels.Faq;
using CRMS.Views.User.TicketsPage;
using CRMS.ViewModels.UserVM;
using CRMS.Views.User.TicketEdit;
using CRMS.Views.Support;
using CRMS.ViewModels.Support;
using CRMS.Views.AD;
using CRMS.ViewModels.AD;
using CRMS.Business.Services.GroupService;
using CRMS.ViewModels.Admin.Groups;
using CRMS.Views.Admin.Groups;
using CRMS.Infrastructure.Converters;
using CRMS.Business.Services.EmailService;
using CRMS.Views.Dialogs;
using CRMS.Business.Services.FaqService;
using CRMS.Business.ActiveDirectoryService;

//admin@bigfirm.by
//27011984Hp

namespace CRMS
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IServiceScopeFactory ScopeFactory { get; private set; }
        public static IHost AppHost { get; private set; }

        private IConfiguration _configuration;
        private const string SettingsFile = "appsettings.json";

        public App()
        {
            LoadConfiguration();
            

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Подключение к БД
                    services.AddDbContext<CRMSDbContext>(options =>
                        options.UseMySql(
                            _configuration.GetConnectionString("DefaultConnection"),
                            new MySqlServerVersion(new System.Version(10, 4))),
                        ServiceLifetime.Scoped); // <-- теперь Scoped

                    // Email config binding
                    services.Configure<EmailSettings>(context.Configuration.GetSection("EmailSettings"));

                    // EmailService с DI-инъекцией EmailSettings
                    services.AddSingleton<IEmailService, EmailService>(sp =>
                    {
                        var emailSettings = sp.GetRequiredService<IOptions<EmailSettings>>().Value;
                        return new EmailService(emailSettings);
                    });
                    
                    // Сервисы и инфраструктура
                    services.AddScoped<IUnitOfWork, EFUnitOfWork>();
                    services.AddScoped<IGroupService, GroupService>();
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IUserService, UserService>();
                    services.AddScoped<ITicketService, TicketService>();
                    services.AddScoped<IAttachmentService, AttachmentService>();
                    services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
                    services.AddScoped<IFaqService, FaqService>();

                    services.AddScoped<INavigationService, NavigationService>();
                    services.AddSingleton<NullToBoolConverter>();
                    // services.AddScoped<IEmailService, EmailService>();

                    // Окна
                    services.AddTransient<StartUpWindow>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<ADLoginWindow>(); // ✅            
                    services.AddTransient<TicketEditWindow>();
                    services.AddTransient<AddUserToGroupWindow>();
                    services.AddTransient<UserProfileWindow>();
                    services.AddTransient<UserCreateWindow>();
                    services.AddTransient<ForgotPasswordWindow>();
                    services.AddTransient<PasswordResetSentWindow>();

                    // Страницы
                    services.AddTransient<MainAdminPage>();
                    services.AddTransient<MainSupportPage>();
                    services.AddTransient<SupportTicketsPage>();
                    services.AddTransient<MainUserPage>();
                    services.AddTransient<UserTicketsPage>();
                    services.AddTransient<UsersOverviewPage>();
                    services.AddTransient<GroupManagerPage>();
                    services.AddTransient<FaqAdminPage>();
                    services.AddTransient<FaqDetailPage>();
                    services.AddTransient<FaqPage>();

                    // ViewModels
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<LoginWindowViewModel>();
                    services.AddTransient<StartUpWindowViewModel>();

                    services.AddTransient<UserEditWindowViewModel>();

                    services.AddTransient<UserTicketsViewModel>();
                    services.AddTransient<TicketEditViewModel>();
                    services.AddTransient<SupportTicketsViewModel>();

                    services.AddTransient<ADLoginWindowViewModel>();

                    services.AddTransient<UsersOverviewPageViewModel>();
                    services.AddTransient<UserProfileWindowViewModel>();
                    services.AddTransient<UserCreateWindowViewModel>();

                    services.AddTransient<CreateGroupViewModel>();
                    services.AddTransient<GroupOverviewViewModel>();
                    services.AddTransient<AddUserToGroupViewModel>();
                    services.AddTransient<EditDeleteGroupViewModel>();

                    services.AddTransient<FaqAdminPageViewModel>();
                    services.AddTransient<FaqDetailPageViewModel>();
                    services.AddTransient<FaqPageViewModel>();

                })
                .Build();

            ServiceProvider = AppHost.Services;
            ScopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            AppHost.Start();
        }

        //private void ConfigureServices(ServiceCollection services)
        //{
                        
        //}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = ServiceProvider.GetRequiredService<StartUpWindow>();
            mainWindow.Show();

            // Глобальный обработчик ошибок
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Ошибка: {args.Exception.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }

        private void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(SettingsFile, optional: false, reloadOnChange: true);
            _configuration = builder.Build();
        }


        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);
        //    LoadConfiguration();
        //    bool isDarkTheme = LoadThemeSetting();
        //    ApplyTheme(isDarkTheme);

        //    // Загружаем строку подключения из конфигурации
        //    string connectionString = _configuration.GetConnectionString("DefaultConnection");

        //    // Создаём сервис окон
        //    var windowService = new WindowService();

        //    // Создаём экземпляр UnitOfWork с корректной строкой подключения
        //    var unitOfWork = new EFUnitOfWork(connectionString);

        //    // Создаём UserManager, передавая ему UnitOfWork
        //    var userManager = new UserManager(unitOfWork);

        //    // Устанавливаем DataContext для MainWindow
        //    var mainWindow = new MainWindow
        //    {
        //        DataContext = new MainWindowViewModel(userManager, windowService)
        //    };
        //    mainWindow.Show();
        //}

        //public bool LoadThemeSetting()
        //{
        //    return bool.TryParse(_configuration["AppSettings:IsDarkTheme"], out bool isDark) && isDark;
        //}

        //public void SaveThemeSetting(bool isDarkTheme)
        //{
        //    var json = File.ReadAllText(SettingsFile);
        //    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        //    jsonObj["AppSettings"]["IsDarkTheme"] = isDarkTheme;
        //    File.WriteAllText(SettingsFile, Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented));
        //}

        //public void ApplyTheme(bool isDark)
        //{
        //    var paletteHelper = new PaletteHelper();
        //    MaterialDesignThemes.Wpf.Theme theme = paletteHelper.GetTheme();
        //    theme.SetBaseTheme(isDark ? MaterialDesignThemes.Wpf.BaseTheme.Dark : MaterialDesignThemes.Wpf.BaseTheme.Light);
        //    paletteHelper.SetTheme(theme);

        //    ThemeManager.Current.ChangeTheme(this, isDark ? "Dark.Blue" : "Light.Blue");
        //}
    }
}
