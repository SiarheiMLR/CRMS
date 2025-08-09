using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CRMS.Services
{
    /// <summary>
    /// Сервис для управления навигацией между страницами.
    /// </summary>
    public class NavigationService : INavigationService
    {
        /// <summary>
        /// Фрейм, используемый для навигации между страницами.
        /// </summary>
        private Frame _frame;

        /// <summary>
        /// Устанавливает фрейм, который будет использоваться для навигации.
        /// </summary>
        /// <param name="frame">Фрейм, в котором будет происходить навигация.</param>
        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        /// <summary>
        /// Переходит на страницу указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип страницы, на которую нужно перейти. Должен быть производным от <see cref="Page"/>.</typeparam>
        public void NavigateTo<T>() where T : Page
        {
            // Получаем экземпляр страницы через DI-контейнер
            var page = App.ServiceProvider.GetRequiredService<T>();
            // Переходим на страницу
            _frame.Navigate(page);
        }

        public void NavigateToWithParameter<T>(object parameter) where T : Page
        {
            var page = App.ServiceProvider.GetRequiredService<T>();
            _frame.Navigate(page, parameter);
        }
    }
}