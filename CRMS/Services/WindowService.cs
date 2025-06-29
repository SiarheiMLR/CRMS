using System.Windows;
using CRMS.Views;

namespace CRMS.Services
{
    public class WindowService : IWindowService
    {
        public void ShowWindow<T>() where T : new()
        {
            var window = new T() as Window;
            window?.Show();
        }

        public void CloseWindow<T>()
        {
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault() as Window;
            window?.Close();
        }
    }
}
