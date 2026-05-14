using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using up.Infrastructure;
using up.Models;

namespace up.ViewModels
{
    /// <summary>
    /// ViewModel формы создания и редактирования книги.
    /// Поддерживает работу с текстом, описанием, обложкой и привязкой жанров.
    /// </summary>
    public class BookEditorViewModel : ViewModelBase
    {
        private int? _bookId;
        public bool IsEditMode => _bookId.HasValue;

        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _imageUrl;
        public string ImageUrl { get => _imageUrl; set { _imageUrl = value; OnPropertyChanged(); } }

        private string _text;
        public string Text { get => _text; set { _text = value; OnPropertyChanged(); } }

        public ObservableCollection<GenreItem> AvailableGenres { get; } = new ObservableCollection<GenreItem>();

        /// <summary>Команда сохранения изменений или создания новой книги.</summary>
        public RelayCommand SaveCommand { get; }
        /// <summary>Команда отмены и возврата на страницу автора.</summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>
        /// Инициализирует форму. Если передан bookId, загружает существующие данные.
        /// </summary>
        /// <param name="bookId">Идентификатор книги для редактирования или null для создания.</param>
        public BookEditorViewModel(int? bookId)
        {
            _bookId = bookId;
            LoadGenres();
            if (IsEditMode) LoadBook();

            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => NavigationService.Navigate(new AuthorViewModel()));
        }

        /// <summary>
        /// Загружает список всех жанров из БД и отмечает выбранные для текущей книги (если редактирование).
        /// </summary>
        private void LoadGenres()
        {
            var genres = Core.Context.Genres.Select(g => new GenreItem { Id = g.ID, Name = g.GenreName }).ToList();

            if (IsEditMode)
            {
                var bookGenreIds = Core.Context.BooksGenres
                    .Where(bg => bg.BookId == _bookId.Value)
                    .Select(bg => bg.GenreId)
                    .ToList();

                foreach (var genre in genres)
                    genre.IsSelected = bookGenreIds.Contains(genre.Id);
            }

            foreach (var genre in genres)
                AvailableGenres.Add(genre);
        }

        /// <summary>
        /// Загружает данные существующей книги в поля формы.
        /// </summary>
        private void LoadBook()
        {
            var book = Core.Context.Books.Find(_bookId.Value);
            if (book == null) return;

            Title = book.BookName;
            Description = book.Description;
            ImageUrl = book.ImageURL;
            Text = book.Text;
        }

        /// <summary>
        /// Проверяет валидность данных перед сохранением.
        /// </summary>
        /// <returns>True, если заполнены обязательные поля (Название и Текст).</returns>
        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Text);
        }

        /// <summary>
        /// Сохраняет книгу в БД: создаёт новую запись или обновляет существующую. 
        /// Синхронизирует связи с таблицей жанров.
        /// </summary>
        private void ExecuteSave()
        {
            try
            {
                if (IsEditMode)
                {
                    var book = Core.Context.Books.Find(_bookId.Value);
                    book.BookName = Title;
                    book.Description = Description;
                    book.ImageURL = ImageUrl;
                    book.Text = Text;
                }
                else
                {
                    var newBook = new Books
                    {
                        BookName = Title,
                        Description = Description,
                        ImageURL = ImageUrl,
                        Text = Text,
                        UserId = UserSession.UserId,
                        IsFrozen = false
                    };
                    Core.Context.Books.Add(newBook);
                    Core.Context.SaveChanges();
                    _bookId = newBook.ID; 
                }

                var existingGenres = Core.Context.BooksGenres.Where(bg => bg.BookId == _bookId.Value).ToList();
                Core.Context.BooksGenres.RemoveRange(existingGenres);

                var selectedGenreIds = AvailableGenres.Where(g => g.IsSelected).Select(g => g.Id).ToList();
                foreach (var genreId in selectedGenreIds)
                {
                    Core.Context.BooksGenres.Add(new BooksGenres { BookId = _bookId.Value, GenreId = genreId });
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Книга успешно сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new AuthorViewModel());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}