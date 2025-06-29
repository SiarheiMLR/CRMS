using System.Windows.Input;

namespace CRMS.Commands
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Признак CanExecute
        /// </summary>
        private Func<object, bool> canExecute;

        /// <summary>
        /// делегат метода Execute
        /// </summary>
        private Action<object> executeMethod;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="executeMethod">делегат метода Execute</param>
        /// <param name="canExecute">делегат признака Can Execute</param>
        public RelayCommand(Action<object> executeMethod, Func<object, bool> canExecute)
        {
            this.executeMethod = executeMethod;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="executeMethod">делегат метода Execute</param>
        public RelayCommand(Action<object> executeMethod)
        {
            this.executeMethod = executeMethod;
        }
        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke(parameter) ?? true;
        }
        public void Execute(object parameter)
        {
            executeMethod(parameter);
        }
    }
}
