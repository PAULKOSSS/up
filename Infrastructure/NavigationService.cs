using System;

namespace up.Infrastructure
{
    /// <summary>
    /// Статический сервис навигации, используемый в архитектуре MVVM.
    /// Позволяет ViewModel инициировать смену представления без прямой ссылки на UI-контролы.
    /// </summary>
    public static class NavigationService
    {
        /// <summary>
        /// Делегат для выполнения навигации. 
        /// Инициализируется в MainViewModel при старте приложения.
        /// Принимает объект ViewModel или View для отображения.
        /// </summary>
        public static Action<object> Navigate { get; set; }
    }
}