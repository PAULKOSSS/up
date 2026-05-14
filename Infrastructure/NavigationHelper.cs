using System.Windows.Controls;

namespace up.Infrastructure
{
    /// <summary>
    /// Вспомогательный статический класс для выполнения навигации внутри элемента Frame в WPF.
    /// </summary>
    public static class NavigationHelper
    {
        /// <summary>
        /// Ссылка на элемент Frame, в который выполняется навигация.
        /// Должен быть инициализирован при запуске приложения.
        /// </summary>
        public static Frame ContentFrame { get; set; }

        /// <summary>
        /// Выполняет переход к указанному объекту представления (UserControl, Page и т.д.).
        /// </summary>
        /// <param name="view">Объект представления, к которому нужно перейти.</param>
        public static void Navigate(object view)
        {
            ContentFrame?.Navigate(view);
        }
    }
}