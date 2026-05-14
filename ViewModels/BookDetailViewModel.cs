using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using up.Infrastructure;

namespace up.ViewModels
{
    /// <summary>
    /// DTO-модель для отображения отзыва на странице книги.
    /// Используется для привязки данных в списке отзывов без загрузки всей сущности Reviews.
    /// </summary>
    public class ReviewCard
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AuthorName { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }
    }

    /// <summary>
    /// ViewModel для страницы детального просмотра книги.
    /// Отвечает за загрузку данных книги, управление отзывами, рейтингом, 
    /// подачу жалоб и добавление книги в личный список чтения.
    /// </summary>
    public class BookDetailViewModel : ViewModelBase
    {
        private readonly int _bookId;

        #region Свойства книги
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _imageUrl;
        public string ImageUrl { get => _imageUrl; set { _imageUrl = value; OnPropertyChanged(); } }

        private string _authorName;
        public string AuthorName { get => _authorName; set { _authorName = value; OnPropertyChanged(); } }

        private string _genres;
        public string Genres { get => _genres; set { _genres = value; OnPropertyChanged(); } }

        private string _bookText;
        public string BookText { get => _bookText; set { _bookText = value; OnPropertyChanged(); } }

        private double _averageRating;
        public double AverageRating { get => _averageRating; set { _averageRating = value; OnPropertyChanged(); } }

        private bool _isBookFrozen;
        public bool IsBookFrozen { get => _isBookFrozen; set { _isBookFrozen = value; OnPropertyChanged(); } }
        #endregion

        #region Отзывы
        private ObservableCollection<ReviewCard> _reviews;
        public ObservableCollection<ReviewCard> Reviews { get => _reviews; set { _reviews = value; OnPropertyChanged(); } }

        private string _newReviewText;
        public string NewReviewText { get => _newReviewText; set { _newReviewText = value; OnPropertyChanged(); } }

        private int _newReviewRating = 5;
        public int NewReviewRating { get => _newReviewRating; set { _newReviewRating = value; OnPropertyChanged(); } }
        #endregion

        public bool IsAdmin => UserSession.IsAdmin;

        #region Команды
        /// <summary>Команда подачи жалобы на книгу.</summary>
        public RelayCommand ReportBookCommand { get; }
        /// <summary>Команда подачи жалобы на автора книги.</summary>
        public RelayCommand ReportAuthorCommand { get; }
        /// <summary>Команда добавления нового отзыва.</summary>
        public RelayCommand AddReviewCommand { get; }
        /// <summary>Команда заморозки/разморозки книги (только для админа).</summary>
        public RelayCommand FreezeBookCommand { get; }
        /// <summary>Команда подачи жалобы на конкретный отзыв.</summary>
        public RelayCommand ReportReviewCommand { get; }
        /// <summary>Команда добавления книги в список чтения.</summary>
        public RelayCommand AddToListCommand { get; }
        #endregion

        /// <summary>
        /// Инициализирует ViewModel и загружает данные книги по её идентификатору.
        /// </summary>
        /// <param name="bookId">Идентификатор книги в базе данных.</param>
        public BookDetailViewModel(int bookId)
        {
            _bookId = bookId;
            LoadData();

            ReportBookCommand = new RelayCommand(_ => ExecuteReport("Книга", null));
            ReportAuthorCommand = new RelayCommand(_ => ExecuteReport("Автор", null));
            AddReviewCommand = new RelayCommand(_ => ExecuteAddReview(), _ => !string.IsNullOrWhiteSpace(NewReviewText));
            FreezeBookCommand = new RelayCommand(_ => ExecuteFreezeBook(), _ => IsAdmin);
            ReportReviewCommand = new RelayCommand(id => ExecuteReport("Отзыв", (int?)id));
            AddToListCommand = new RelayCommand(_ => ExecuteAddToList());
        }

        /// <summary>
        /// Загружает информацию о книге, авторе, жанрах, отзывах и вычисляет средний рейтинг.
        /// </summary>
        private void LoadData()
        {
            var book = Core.Context.Books.FirstOrDefault(b => b.ID == _bookId);
            if (book == null) return;

            Title = book.BookName;
            Description = book.Description;
            ImageUrl = book.ImageURL;
            BookText = book.Text;
            IsBookFrozen = book.IsFrozen == true;

            var author = Core.Context.Users.FirstOrDefault(u => u.ID == book.UserId);
            AuthorName = author?.Nickname ?? "Неизвестный";

            var genres = Core.Context.BooksGenres
                .Where(bg => bg.BookId == _bookId)
                .Select(bg => bg.Genres.GenreName)
                .ToList();
            Genres = string.Join(", ", genres);

            var reviewsData = Core.Context.Reviews
                .Where(r => r.BookId == _bookId)
                .Select(r => new ReviewCard
                {
                    Id = r.ID,
                    UserId = r.UserId ?? 0,
                    AuthorName = r.Users.Nickname,
                    Text = r.Text,
                    Rating = r.Rating
                }).ToList();

            Reviews = new ObservableCollection<ReviewCard>(reviewsData);
            AverageRating = reviewsData.Any() ? reviewsData.Average(r => r.Rating) : 0;
        }

        /// <summary>
        /// Создаёт запись жалобы на книгу, автора или отзыв.
        /// </summary>
        /// <param name="targetType">Тип объекта жалобы: "Книга", "Автор" или "Отзыв".</param>
        /// <param name="reviewId">Идентификатор отзыва (если жалоба на отзыв).</param>
        private void ExecuteReport(string targetType, int? reviewId)
        {
            string reason = Microsoft.VisualBasic.Interaction.InputBox(
                $"Укажите причину жалобы на {targetType.ToLower()}:",
                "Жалоба",
                " ");

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Жалоба не отправлена: не указана причина.", "Отмена", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? authorId = null;

            if (targetType == "Автор")
            {
                var book = Core.Context.Books.Find(_bookId);
                if (book != null) authorId = book.UserId;
            }

            var complaint = new Complaints
            {
                UserId = UserSession.UserId,
                BookId = targetType == "Книга" ? _bookId : (int?)null,
                ReviewId = reviewId,
                AuthorId = authorId,
                Reason = reason,
                CreatedAt = DateTime.Now
            };

            Core.Context.Complaints.Add(complaint);
            Core.Context.SaveChanges();

            MessageBox.Show($"Жалоба на {targetType.ToLower()} отправлена администратору.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Сохраняет новый отзыв в базе данных и обновляет список отзывов.
        /// </summary>
        private void ExecuteAddReview()
        {
            Core.Context.Reviews.Add(new Reviews
            {
                UserId = UserSession.UserId,
                BookId = _bookId,
                Text = NewReviewText,
                Rating = NewReviewRating,
                ReviewDate = DateTime.Now
            });
            Core.Context.SaveChanges();

            NewReviewText = "";
            NewReviewRating = 5;
            LoadData(); 
        }

        /// <summary>
        /// Переключает статус заморозки книги (доступно только администратору).
        /// </summary>
        private void ExecuteFreezeBook()
        {
            var book = Core.Context.Books.Find(_bookId);
            book.IsFrozen = !book.IsFrozen;
            Core.Context.SaveChanges();
            IsBookFrozen = book.IsFrozen == true;
            MessageBox.Show(IsBookFrozen ? "Книга заморожена" : "Книга разморожена", "Администрирование", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Добавляет книгу в список чтения текущего пользователя со статусом "В планах".
        /// </summary>
        private void ExecuteAddToList()
        {
            try
            {
                var exists = Core.Context.ReadingLists
                    .Any(rl => rl.UserId == UserSession.UserId && rl.BookId == _bookId);

                if (exists)
                {
                    MessageBox.Show("Книга уже находится в вашем списке!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var newListEntry = new ReadingLists
                {
                    UserId = UserSession.UserId,
                    BookId = _bookId,
                    BookStatusId = 2, 
                    AddedDate = DateTime.Now
                };

                Core.Context.ReadingLists.Add(newListEntry);
                Core.Context.SaveChanges();

                MessageBox.Show("Книга добавлена в список 'В планах'", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}