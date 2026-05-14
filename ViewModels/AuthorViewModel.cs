using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using up.Infrastructure;
using up.Models;

namespace up.ViewModels
{
    /// <summary>
    /// ViewModel панели управления книгами для роли "Автор".
    /// Отображает все опубликованные книги, выделяет замороженные и позволяет подавать заявки на их разблокировку.
    /// </summary>
    public class AuthorViewModel : ViewModelBase
    {
        private ObservableCollection<BookCard> _allBooks;
        public ObservableCollection<BookCard> AllBooks
        {
            get => _allBooks;
            set { _allBooks = value; OnPropertyChanged(); }
        }

        private ObservableCollection<BookCard> _frozenBooks;
        public ObservableCollection<BookCard> FrozenBooks
        {
            get => _frozenBooks;
            set { _frozenBooks = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasFrozenBooks)); }
        }

        public bool HasFrozenBooks => FrozenBooks != null && FrozenBooks.Count > 0;

        /// <summary>Команда перехода к форме создания новой книги.</summary>
        public RelayCommand AddBookCommand { get; }
        /// <summary>Команда подачи заявки на снятие заморозки конкретной книги.</summary>
        public RelayCommand AppealBookFreezeCommand { get; }

        /// <summary>
        /// Инициализирует коллекции, команды и загружает книги автора из БД.
        /// </summary>
        public AuthorViewModel()
        {
            AllBooks = new ObservableCollection<BookCard>();
            FrozenBooks = new ObservableCollection<BookCard>();

            AddBookCommand = new RelayCommand(_ => ExecuteAddBook());
            AppealBookFreezeCommand = new RelayCommand(book => ExecuteAppealBookFreeze(book as BookCard));

            LoadMyBooks();
        }

        /// <summary>
        /// Загружает книги текущего пользователя из БД и разделяет их на обычные и замороженные.
        /// </summary>
        private void LoadMyBooks()
        {
            var books = Core.Context.Books
                .Where(b => b.UserId == UserSession.UserId)
                .ToList();

            AllBooks.Clear();
            FrozenBooks.Clear();

            foreach (var b in books)
            {
                var card = new BookCard
                {
                    Id = b.ID,
                    Title = b.BookName,
                    AuthorName = b.Users.Nickname,
                    ImageUrl = b.ImageURL,
                    IsFrozen = b.IsFrozen == true
                };

                AllBooks.Add(card);
                if (card.IsFrozen)
                    FrozenBooks.Add(card);
            }
        }

        /// <summary>
        /// Открывает форму редактора книги в режиме создания новой записи.
        /// </summary>
        private void ExecuteAddBook()
        {
            NavigationService.Navigate(new BookEditorViewModel(null));
        }

        /// <summary>
        /// Создаёт заявку на снятие заморозки для выбранной книги.
        /// </summary>
        /// <param name="book">Карточка книги, для которой подаётся заявка.</param>
        private void ExecuteAppealBookFreeze(BookCard book)
        {
            if (book == null) return;

            string reason = Microsoft.VisualBasic.Interaction.InputBox(
                $"Укажите причину для разблокировки книги \"{book.Title}\":",
                "Заявка на снятие заморозки", " ");

            if (!string.IsNullOrWhiteSpace(reason))
            {
                Core.Context.UnfreezeApplications.Add(new UnfreezeApplications
                {
                    UserId = UserSession.UserId,
                    TargetTypeId = 2, 
                    TargetBookId = book.Id,
                    Reason = reason,
                    ApplicationDate = DateTime.Now,
                    Status = "Pending"
                });

                Core.Context.SaveChanges();
                MessageBox.Show("Заявка на разблокировку книги отправлена администратору!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}