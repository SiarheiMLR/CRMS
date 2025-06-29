using System.Windows.Controls;

namespace CRMS.Services
{
    /// <summary>
    /// Интерфейс для управления навигацией между страницами.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Переходит на страницу указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип страницы, на которую нужно перейти. Должен быть производным от <see cref="Page"/>.</typeparam>
        void NavigateTo<T>() where T : Page;

        /// <summary>
        /// Устанавливает фрейм, который будет использоваться для навигации.
        /// </summary>
        /// <param name="frame">Фрейм, в котором будет происходить навигация.</param>
        void SetFrame(Frame frame);
    }
}