using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using up.Infrastructure;
using up.Models;

namespace up.ViewModels
{
    /// <summary>
    /// ViewModel профиля пользователя. Загружает личные данные, историю отзывов, 
    /// управляет заявками на роль автора и апелляциями при заморозке аккаунта.
    /// </summary>
    public class ProfileViewModel : ViewModelBase
    {
        private string _nickname;
        public string Nickname { get => _nickname; set { _nickname = value; OnPropertyChanged(); } }

        private string _login;
        public string Login { get => _login; set { _login = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _roleName;
        public string RoleName { get => _roleName; set { _roleName = value; OnPropertyChanged(); } }

        private bool _isFrozen;
        public bool IsFrozen { get => _isFrozen; set { _isFrozen = value; OnPropertyChanged(); } }

        private string _freezeReason;
        public string FreezeReason { get => _freezeReason; set { _freezeReason = value; OnPropertyChanged(); } }

        private ObservableCollection<ReviewDto> _reviews;
        public ObservableCollection<ReviewDto> Reviews { get => _reviews; set { _reviews = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoReviews)); } }

        public bool HasNoReviews => Reviews == null || Reviews.Count == 0;
        public bool CanRequestAuthor => RoleName == "Читатель";

        private bool _isAuthorRequestPending;
        public bool IsAuthorRequestPending
        {
            get => _isAuthorRequestPending;
            set { _isAuthorRequestPending = value; OnPropertyChanged(); }
        }

        public bool IsAuthorRequestButtonVisible => CanRequestAuthor && !IsAuthorRequestPending;

        /// <summary>Команда подачи заявки на получение роли "Автор".</summary>
        public RelayCommand RequestAuthorCommand { get; }
        /// <summary>Команда подачи апелляции на снятие заморозки аккаунта.</summary>
        public RelayCommand AppealFreezeCommand { get; }

        /// <summary>
        /// Инициализирует команды и загружает все необходимые данные профиля из БД.
        /// </summary>
        public ProfileViewModel()
        {
            RequestAuthorCommand = new RelayCommand(_ => ExecuteRequestAuthor());
            AppealFreezeCommand = new RelayCommand(_ => ExecuteAppealFreeze());

            LoadUserData();
            LoadReviews();
            CheckAuthorRequestStatus();
        }

        /// <summary>
        /// Загружает данные текущего пользователя, его роль и причину заморозки (если применимо).
        /// </summary>
        private void LoadUserData()
        {
            var user = Core.Context.Users.FirstOrDefault(u => u.ID == UserSession.UserId);
            if (user == null) return;

            var role = Core.Context.Roles.FirstOrDefault(r => r.ID == user.RoleId);

            Nickname = user.Nickname;
            Login = user.Login;
            Email = user.Email;
            RoleName = role?.RoleName ?? "Неизвестно";
            IsFrozen = user.IsFrozen == true;

            if (IsFrozen)
            {
                var complaint = Core.Context.Complaints
                    .Where(c => c.UserId == UserSession.UserId ||
                               (c.ReviewId.HasValue && Core.Context.Reviews.Any(r => r.ID == c.ReviewId.Value && r.UserId == UserSession.UserId)) ||
                               (c.BookId.HasValue && Core.Context.Books.Any(b => b.ID == c.BookId.Value && b.UserId == UserSession.UserId)))
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefault();

                FreezeReason = complaint?.Reason ?? "Аккаунт заморожен администратором";
            }
        }

        /// <summary>
        /// Загружает список отзывов пользователя, отсортированный по дате создания.
        /// </summary>
        private void LoadReviews()
        {
            var userReviews = Core.Context.Reviews
                .Where(r => r.UserId == UserSession.UserId)
                .Select(r => new ReviewDto
                {
                    BookName = r.Books.BookName,
                    Text = r.Text,
                    Rating = r.Rating,
                    Date = r.ReviewDate
                })
                .OrderByDescending(r => r.Date)
                .ToList();

            Reviews = new ObservableCollection<ReviewDto>(userReviews);
        }

        /// <summary>
        /// Проверяет наличие активной заявки на роль автора в базе данных.
        /// </summary>
        private void CheckAuthorRequestStatus()
        {
            IsAuthorRequestPending = Core.Context.RoleApplications
                .Any(a => a.UserId == UserSession.UserId && a.Status == "Pending");
        }

        /// <summary>
        /// Создаёт новую заявку на роль автора со статусом "Pending".
        /// </summary>
        private void ExecuteRequestAuthor()
        {
            if (IsAuthorRequestPending)
            {
                MessageBox.Show("У вас уже есть активная заявка на роль автора.", "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Core.Context.RoleApplications.Add(new RoleApplications
            {
                UserId = UserSession.UserId,
                ApplicationDate = DateTime.Now,
                Status = "Pending"
            });
            Core.Context.SaveChanges();

            IsAuthorRequestPending = true;
            MessageBox.Show("Заявка на роль автора отправлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Открывает диалоговое окно для ввода причины и создаёт заявку на разморозку аккаунта.
        /// </summary>
        private void ExecuteAppealFreeze()
        {
            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину для разблокировки:", "Заявка на снятие заморозки", " ");
            if (!string.IsNullOrWhiteSpace(reason))
            {
                Core.Context.UnfreezeApplications.Add(new UnfreezeApplications
                {
                    UserId = UserSession.UserId,
                    TargetTypeId = 1, 
                    TargetBookId = null,
                    Reason = reason,
                    ApplicationDate = DateTime.Now,
                    Status = "Pending"
                });
                Core.Context.SaveChanges();
                MessageBox.Show("Заявка на снятие заморозки отправлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}