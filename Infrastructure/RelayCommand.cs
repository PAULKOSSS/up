using System;
using System.Windows.Input;

namespace up.Infrastructure
{
    /// <summary>
    /// Базовая реализация интерфейса ICommand для привязки команд в WPF.
    /// Инкапсулирует логику выполнения и проверки доступности действия.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Инициализирует новый экземпляр команды.
        /// </summary>
        /// <param name="execute">Делегат, содержащий логику выполнения команды.</param>
        /// <param name="canExecute">Делегат, определяющий возможность выполнения команды. 
        /// Если не указан, команда считается всегда доступной.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если параметр execute равен null.</exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Определяет, можно ли выполнить команду в текущем состоянии приложения.
        /// </summary>
        /// <param name="parameter">Параметр, передаваемый команде (может быть null).</param>
        /// <returns>True, если команда может быть выполнена; иначе false.</returns>
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Выполняет логику команды.
        /// </summary>
        /// <param name="parameter">Параметр, передаваемый команде (может быть null).</param>
        public void Execute(object parameter) => _execute(parameter);

        /// <summary>
        /// Событие, сигнализирующее об изменении состояния доступности команды.
        /// Автоматически привязывается к CommandManager.RequerySuggested для своевременного обновления UI.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}