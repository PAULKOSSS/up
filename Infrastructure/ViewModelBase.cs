using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace up.Infrastructure
{
    /// <summary>
    /// Базовый абстрактный класс для ViewModel, реализующий интерфейс INotifyPropertyChanged.
    /// Обеспечивает механизм уведомления представления об изменении значений свойств.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие, возникающее при изменении значения свойства.
        /// Подписывается представлением для обновления привязанных элементов.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызывает событие PropertyChanged для указанного свойства.
        /// </summary>
        /// <param name="propertyName">Имя изменившегося свойства. 
        /// Если параметр опущен, используется имя вызывающего члена благодаря атрибуту CallerMemberName.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}