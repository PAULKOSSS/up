using up.Infrastructure;
using up.Views;

namespace up.ViewModels
{
    /// <summary>
    /// Главная ViewModel приложения. Управляет навигацией между экранами, 
    /// видимостью элементов интерфейса в зависимости от роли пользователя и состоянием сессии.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private object _currentView;
        /// <summary>
        /// Текущее активное представление (ViewModel), отображаемое в основном окне.
        /// </summary>
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        /// <summary>Команда перехода в каталог книг.</summary>
        public RelayCommand NavigateCatalogCommand { get; }
        /// <summary>Команда перехода в личные списки чтения.</summary>
        public RelayCommand NavigateListsCommand { get; }
        /// <summary>Команда перехода в панель администратора.</summary>
        public RelayCommand NavigateAdminCommand { get; }
        /// <summary>Команда перехода в панель управления книгами автора.</summary>
        public RelayCommand NavigateAuthorCommand { get; }
        /// <summary>Команда перехода в профиль пользователя.</summary>
        public RelayCommand NavigateProfileCommand { get; }
        /// <summary>Команда выхода из системы (завершение сессии).</summary>
        public RelayCommand LogoutCommand { get; }

        public bool ShowAdmin => UserSession.IsAdmin;
        public bool ShowAuthor => UserSession.IsAuthor;
        public bool ShowFrozenWarning => UserSession.IsFrozen;

        /// <summary>
        /// Инициализирует команды навигации, устанавливает представление по умолчанию 
        /// и настраивает глобальный сервис навигации для использования из других ViewModel.
        /// </summary>
        public MainViewModel()
        {
            NavigateCatalogCommand = new RelayCommand(_ => CurrentView = new CatalogViewModel());
            NavigateListsCommand = new RelayCommand(_ => CurrentView = new BookListsViewModel());
            NavigateAdminCommand = new RelayCommand(_ => CurrentView = new AdminViewModel());
            NavigateAuthorCommand = new RelayCommand(_ => CurrentView = new AuthorViewModel());
            NavigateProfileCommand = new RelayCommand(_ => CurrentView = new ProfileViewModel());
            LogoutCommand = new RelayCommand(ExecuteLogout);

            CurrentView = new CatalogViewModel();

            NavigationService.Navigate = view => CurrentView = view;
        }

        /// <summary>
        /// Выполняет выход из системы: очищает данные сессии, сбрасывает текущее представление 
        /// и перенаправляет пользователя на экран авторизации.
        /// </summary>
        /// <param name="obj">Параметр команды (не используется).</param>
        private void ExecuteLogout(object obj)
        {
            UserSession.UserId = 0;
            UserSession.Login = null;
            UserSession.RoleName = null;
            UserSession.IsFrozen = false;

            CurrentView = null; 
            NavigationHelper.Navigate(new AuthView());
        }
    }
}