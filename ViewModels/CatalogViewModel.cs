using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using up.Infrastructure;
using up.Models;

namespace up.ViewModels
{
    /// <summary>
    /// ViewModel главного каталога книг. Реализует поиск с задержкой (debounce), 
    /// фильтрацию по жанрам и сортировку результатов.
    /// </summary>
    public class CatalogViewModel : ViewModelBase
    {
        private ObservableCollection<BookCard> _books;
        public ObservableCollection<BookCard> Books { get => _books; set { _books = value; OnPropertyChanged(); } }

        private string _searchText;
        /// <summary>
        /// Текст поискового запроса. При изменении запускает таймер задержки перед выполнением запроса к БД.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); _searchTimer?.Stop(); _searchTimer?.Start(); }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        { "По названию (А-Я)", "По названию (Я-А)", "По автору (А-Я)", "По автору (Я-А)", "По рейтингу (по убыв.)", "По рейтингу (по возр.)" };

        private string _selectedSortOption;
        public string SelectedSortOption
        { get => _selectedSortOption; set { _selectedSortOption = value; OnPropertyChanged(); ApplySorting(); } }

        public ObservableCollection<string> AvailableGenres { get; } = new ObservableCollection<string>();
        private string _selectedGenre;
        public string SelectedGenre
        { get => _selectedGenre; set { _selectedGenre = value; OnPropertyChanged(); LoadBooks(); } }

        /// <summary>Таймер для реализации задержки поиска (debounce 500мс).</summary>
        private readonly DispatcherTimer _searchTimer;
        private List<BookCard> _loadedBooks;

        /// <summary>Команда перехода на страницу детального просмотра книги.</summary>
        public RelayCommand OpenBookCommand { get; }

        /// <summary>
        /// Инициализирует таймер поиска, загружает жанры, устанавливает параметры по умолчанию 
        /// и выполняет первичную загрузку каталога.
        /// </summary>
        public CatalogViewModel()
        {
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); LoadBooks(); };

            AvailableGenres.Add("Все жанры");
            LoadGenres();

            SelectedSortOption = SortOptions[0];
            SelectedGenre = "Все жанры";

            OpenBookCommand = new RelayCommand(id => NavigationService.Navigate(new BookDetailViewModel((int)id)));
            LoadBooks();
        }

        /// <summary>
        /// Загружает уникальные названия жанров из базы данных.
        /// </summary>
        private void LoadGenres()
        {
            var genres = Core.Context.Genres.Select(g => g.GenreName).Distinct().ToList();
            foreach (var g in genres) AvailableGenres.Add(g);
        }

        /// <summary>
        /// Выполняет LINQ-запрос к БД с учётом поискового запроса и фильтра по жанру.
        /// Результаты сохраняются в кэш для последующей сортировки.
        /// </summary>
        private void LoadBooks()
        {
            var query = Core.Context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(b => b.BookName.Contains(SearchText) || b.Users.Nickname.Contains(SearchText));

            if (SelectedGenre != "Все жанры")
                query = query.Where(b => b.BooksGenres.Any(bg => bg.Genres.GenreName == SelectedGenre));

            _loadedBooks = query.Select(b => new BookCard
            {
                Id = b.ID,
                Title = b.BookName,
                AuthorName = b.Users.Nickname,
                ImageUrl = b.ImageURL,
                Rating = null
            }).ToList();

            ApplySorting();
        }

        /// <summary>
        /// Применяет выбранную сортировку к кэшированному списку книг в памяти.
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
                case "По рейтингу (по убыв.)": sorted = sorted.OrderByDescending(b => b.Rating ?? 0); break;
                case "По рейтингу (по возр.)": sorted = sorted.OrderBy(b => b.Rating ?? 0); break;
            }
            Books = new ObservableCollection<BookCard>(sorted.ToList());
        }
    }
}