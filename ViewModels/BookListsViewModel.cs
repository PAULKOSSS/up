using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using up.Infrastructure;
using up.Models;

namespace up.ViewModels
{
    /// <summary>
    /// ViewModel личных списков чтения. Управляет фильтрацией по статусам, 
    /// поиском, сортировкой и изменением статуса книг в списке пользователя.
    /// </summary>
    public class BookListsViewModel : ViewModelBase
    {
        private ObservableCollection<BookCard> _books;
        public ObservableCollection<BookCard> Books { get => _books; set { _books = value; OnPropertyChanged(); } }

        private int _selectedStatusId = 3;
        public int SelectedStatusId
        {
            get => _selectedStatusId;
            set { _selectedStatusId = value; OnPropertyChanged(); UpdateSectionTitle(); LoadBooks(); }
        }

        private string _currentSectionTitle;
        public string CurrentSectionTitle { get => _currentSectionTitle; set { _currentSectionTitle = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); LoadBooks(); } }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        { "По названию (А-Я)", "По названию (Я-А)", "По автору (А-Я)", "По автору (Я-А)" };

        private string _selectedSortOption;
        public string SelectedSortOption
        { get => _selectedSortOption; set { _selectedSortOption = value; OnPropertyChanged(); ApplySorting(); } }

        public ObservableCollection<string> AvailableGenres { get; } = new ObservableCollection<string>();
        private string _selectedGenre;
        public string SelectedGenre
        { get => _selectedGenre; set { _selectedGenre = value; OnPropertyChanged(); LoadBooks(); } }

        private List<BookCard> _loadedBooks;

        /// <summary>Команда переключения на статус "Заброшено".</summary>
        public RelayCommand SelectAbandonedCommand => new RelayCommand(_ => SelectedStatusId = 1);
        /// <summary>Команда переключения на статус "В планах".</summary>
        public RelayCommand SelectPlannedCommand => new RelayCommand(_ => SelectedStatusId = 2);
        /// <summary>Команда переключения на статус "Читаю".</summary>
        public RelayCommand SelectReadingCommand => new RelayCommand(_ => SelectedStatusId = 3);
        /// <summary>Команда переключения на статус "Прочитано".</summary>
        public RelayCommand SelectReadCommand => new RelayCommand(_ => SelectedStatusId = 4);

        /// <summary>Команда перемещения книги в статус "Читаю".</summary>
        public RelayCommand MoveToReadingCommand { get; }
        /// <summary>Команда перемещения книги в статус "Прочитано".</summary>
        public RelayCommand MoveToFinishedCommand { get; }
        /// <summary>Команда перемещения книги в статус "Заброшено".</summary>
        public RelayCommand MoveToAbandonedCommand { get; }

        /// <summary>
        /// Инициализирует списки жанров, команды изменения статусов и загружает книги по умолчанию.
        /// </summary>
        public BookListsViewModel()
        {
            AvailableGenres.Add("Все жанры");
            LoadGenres();
            SelectedSortOption = SortOptions[0];
            SelectedGenre = "Все жанры";

            MoveToReadingCommand = new RelayCommand(id => ChangeStatus((int)id, 3));
            MoveToFinishedCommand = new RelayCommand(id => ChangeStatus((int)id, 4));
            MoveToAbandonedCommand = new RelayCommand(id => ChangeStatus((int)id, 1));

            UpdateSectionTitle();
            LoadBooks();
        }

        /// <summary>
        /// Обновляет заголовок раздела в зависимости от выбранного статуса.
        /// </summary>
        private void UpdateSectionTitle()
        {
            switch (SelectedStatusId)
            {
                case 1: CurrentSectionTitle = "❌ Заброшено"; break;
                case 2: CurrentSectionTitle = "📅 В планах"; break;
                case 3: CurrentSectionTitle = " Читаю"; break;
                case 4: CurrentSectionTitle = "✅ Прочитано"; break;
            }
        }

        /// <summary>
        /// Загружает уникальные названия жанров из базы данных для фильтрации.
        /// </summary>
        private void LoadGenres()
        {
            var genres = Core.Context.Genres.Select(g => g.GenreName).Distinct().ToList();
            foreach (var g in genres) AvailableGenres.Add(g);
        }

        /// <summary>
        /// Выполняет запрос к БД с учётом статуса, поиска и фильтра по жанру.
        /// Использует Join для получения данных книг и авторов без навигационных свойств.
        /// </summary>
        private void LoadBooks()
        {
            var query = Core.Context.ReadingLists
                .Where(rl => rl.UserId == UserSession.UserId && rl.BookStatusId == SelectedStatusId)
                .Join(Core.Context.Books, rl => rl.BookId, b => b.ID, (rl, b) => new { rl, b })
                .Join(Core.Context.Users, x => x.b.UserId, u => u.ID, (x, u) => new { x, u });

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(item => item.x.b.BookName.Contains(SearchText) || item.u.Nickname.Contains(SearchText));

            if (SelectedGenre != "Все жанры")
                query = query.Where(item => item.x.b.BooksGenres.Any(bg => bg.Genres.GenreName == SelectedGenre));

            _loadedBooks = query.Select(item => new BookCard
            {
                Id = item.x.b.ID,
                Title = item.x.b.BookName,
                AuthorName = item.u.Nickname,
                ImageUrl = item.x.b.ImageURL,
                Rating = null,
                CurrentStatusId = item.x.rl.BookStatusId
            }).ToList();

            ApplySorting();
        }

        /// <summary>
        /// Применяет выбранную сортировку к загруженному списку книг в памяти.
        /// </summary>
        private void ApplySorting()
        {
            if (_loadedBooks == null) return;
            var sorted = _loadedBooks.AsQueryable();

            switch (SelectedSortOption)
            {
                case "По названию (А-Я)": sorted = sorted.OrderBy(b => b.Title); break;
                case "По названию (Я-А)": sorted = sorted.OrderByDescending(b => b.Title); break;
                case "По автору (А-Я)": sorted = sorted.OrderBy(b => b.AuthorName); break;
                case "По автору (Я-А)": sorted = sorted.OrderByDescending(b => b.AuthorName); break;
            }
            Books = new ObservableCollection<BookCard>(sorted.ToList());
        }

        /// <summary>
        /// Изменяет статус книги в списке чтения текущего пользователя и обновляет отображение.
        /// </summary>
        /// <param name="bookId">Идентификатор книги.</param>
        /// <param name="newStatusId">Новый статус (ID из таблицы BookStatuses).</param>
        private void ChangeStatus(int bookId, int newStatusId)
        {
            try
            {
                var item = Core.Context.ReadingLists.FirstOrDefault(rl => rl.UserId == UserSession.UserId && rl.BookId == bookId);
                if (item != null)
                {
                    item.BookStatusId = newStatusId;
                    Core.Context.SaveChanges();
                    LoadBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}