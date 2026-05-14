using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using up.Infrastructure;

namespace up.ViewModels
{
    /// <summary>
    /// ViewModel панели администратора.
    /// Управляет данными пользователей, заявками, замороженными объектами и жалобами.
    /// Предоставляет команды для модерации контента и управления ролями.
    /// </summary>
    public class AdminViewModel : ViewModelBase
    {
        #region Коллекции данных
        private ObservableCollection<Users> _usersList;
        public ObservableCollection<Users> UsersList { get => _usersList; set { _usersList = value; OnPropertyChanged(); } }

        private ObservableCollection<RoleApplications> _roleAppsList;
        public ObservableCollection<RoleApplications> RoleAppsList { get => _roleAppsList; set { _roleAppsList = value; OnPropertyChanged(); } }

        private ObservableCollection<UnfreezeApplications> _unfreezeAppsList;
        public ObservableCollection<UnfreezeApplications> UnfreezeAppsList { get => _unfreezeAppsList; set { _unfreezeAppsList = value; OnPropertyChanged(); } }

        private ObservableCollection<Books> _frozenBooksList;
        public ObservableCollection<Books> FrozenBooksList { get => _frozenBooksList; set { _frozenBooksList = value; OnPropertyChanged(); } }

        private ObservableCollection<Users> _frozenUsersList;
        public ObservableCollection<Users> FrozenUsersList { get => _frozenUsersList; set { _frozenUsersList = value; OnPropertyChanged(); } }

        private ObservableCollection<Complaints> _complaintsList;
        public ObservableCollection<Complaints> ComplaintsList { get => _complaintsList; set { _complaintsList = value; OnPropertyChanged(); } }

        public ObservableCollection<Roles> AllRoles { get; set; }
        #endregion

        #region Команды
        public RelayCommand RefreshCommand { get; }
        public RelayCommand ChangeRoleCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand ToggleUserFreezeCommand { get; }
        public RelayCommand ApproveRoleCommand { get; }
        public RelayCommand RejectRoleCommand { get; }
        public RelayCommand ApproveUnfreezeCommand { get; }
        public RelayCommand RejectUnfreezeCommand { get; }
        public RelayCommand UnfreezeBookCommand { get; }
        public RelayCommand UnfreezeUserCommand { get; }
        public RelayCommand HandleComplaintCommand { get; }
        #endregion

        /// <summary>
        /// Инициализирует коллекции, команды и загружает актуальные данные из БД.
        /// </summary>
        public AdminViewModel()
        {
            UsersList = new ObservableCollection<Users>();
            RoleAppsList = new ObservableCollection<RoleApplications>();
            UnfreezeAppsList = new ObservableCollection<UnfreezeApplications>();
            FrozenBooksList = new ObservableCollection<Books>();
            FrozenUsersList = new ObservableCollection<Users>();
            ComplaintsList = new ObservableCollection<Complaints>();
            AllRoles = new ObservableCollection<Roles>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            ChangeRoleCommand = new RelayCommand(id => ExecuteChangeRole((int)id));
            ChangePasswordCommand = new RelayCommand(id => ExecuteChangePassword((int)id));
            ToggleUserFreezeCommand = new RelayCommand(id => ExecuteToggleUserFreeze((int)id));
            ApproveRoleCommand = new RelayCommand(id => ExecuteApproveRole((int)id));
            RejectRoleCommand = new RelayCommand(id => ExecuteRejectRole((int)id));
            ApproveUnfreezeCommand = new RelayCommand(id => ExecuteApproveUnfreeze((int)id));
            RejectUnfreezeCommand = new RelayCommand(id => ExecuteRejectUnfreeze((int)id));
            UnfreezeBookCommand = new RelayCommand(id => ExecuteUnfreezeBook((int)id));
            UnfreezeUserCommand = new RelayCommand(id => ExecuteUnfreezeUser((int)id));
            HandleComplaintCommand = new RelayCommand(id => ExecuteHandleComplaint((int)id));

            LoadData();
        }

        /// <summary>
        /// Обновляет все коллекции данных из контекста Entity Framework.
        /// </summary>
        private void LoadData()
        {
            UsersList = new ObservableCollection<Users>(Core.Context.Users.ToList());
            RoleAppsList = new ObservableCollection<RoleApplications>(Core.Context.RoleApplications.Where(a => a.Status == "Pending").ToList());
            UnfreezeAppsList = new ObservableCollection<UnfreezeApplications>(Core.Context.UnfreezeApplications.Where(a => a.Status == "Pending").ToList());
            FrozenBooksList = new ObservableCollection<Books>(Core.Context.Books.Where(b => b.IsFrozen == true).ToList());
            FrozenUsersList = new ObservableCollection<Users>(Core.Context.Users.Where(u => u.IsFrozen == true).ToList());
            ComplaintsList = new ObservableCollection<Complaints>(Core.Context.Complaints.OrderByDescending(c => c.CreatedAt).ToList());
            AllRoles = new ObservableCollection<Roles>(Core.Context.Roles.ToList());

            OnPropertyChanged(nameof(UsersList));
            OnPropertyChanged(nameof(RoleAppsList));
            OnPropertyChanged(nameof(UnfreezeAppsList));
            OnPropertyChanged(nameof(FrozenBooksList));
            OnPropertyChanged(nameof(FrozenUsersList));
            OnPropertyChanged(nameof(ComplaintsList));
        }

        #region Управление пользователями
        /// <summary>Изменяет роль указанного пользователя через диалоговое окно ввода.</summary>
        private void ExecuteChangeRole(int userId)
        {
            var user = UsersList.FirstOrDefault(u => u.ID == userId);
            if (user == null) return;

            var currentRole = AllRoles.FirstOrDefault(r => r.ID == user.RoleId);
            string newRoleName = Microsoft.VisualBasic.Interaction.InputBox(
                 "Введите точное название роли (Читатель, Автор, Администратор):",
                 "Смена роли", currentRole?.RoleName ?? "Читатель");

            var newRole = Core.Context.Roles.FirstOrDefault(r => r.RoleName == newRoleName);
            if (newRole != null)
            {
                user.RoleId = newRole.ID;
                Core.Context.SaveChanges();
                MessageBox.Show($"Роль изменена на {newRoleName}");
                LoadData();
            }
            else MessageBox.Show("Роль не найдена.");
        }

        /// <summary>Изменяет пароль пользователя через защищённое диалоговое окно.</summary>
        private void ExecuteChangePassword(int userId)
        {
            var user = UsersList.FirstOrDefault(u => u.ID == userId);
            if (user == null) return;

            string newPass = Microsoft.VisualBasic.Interaction.InputBox("Введите новый пароль:", "Смена пароля");
            if (!string.IsNullOrWhiteSpace(newPass))
            {
                user.Password = newPass;
                Core.Context.SaveChanges();
                MessageBox.Show("Пароль изменён.");
            }
        }

        /// <summary>Переключает статус заморозки аккаунта пользователя.</summary>
        private void ExecuteToggleUserFreeze(int userId)
        {
            var user = UsersList.FirstOrDefault(u => u.ID == userId);
            if (user == null) return;

            user.IsFrozen = !user.IsFrozen;
            Core.Context.SaveChanges();
            MessageBox.Show(user.IsFrozen ? "Пользователь заморожен" : "Пользователь разморожен");
            LoadData();
        }
        #endregion

        #region Заявки на роль
        /// <summary>Одобряет заявку: назначает роль "Автор" и обновляет статус заявки.</summary>
        private void ExecuteApproveRole(int appId)
        {
            var app = RoleAppsList.FirstOrDefault(a => a.ID == appId);
            if (app == null) return;

            var user = Core.Context.Users.Find(app.UserId);
            var authorRole = Core.Context.Roles.FirstOrDefault(r => r.RoleName == "Автор");

            if (user != null && authorRole != null)
            {
                user.RoleId = authorRole.ID;
                app.Status = "Approved";
                Core.Context.SaveChanges();
                MessageBox.Show("Заявка принята. Пользователь стал автором.");
                LoadData();
            }
        }

        /// <summary>Отклоняет заявку на получение роли.</summary>
        private void ExecuteRejectRole(int appId)
        {
            var app = RoleAppsList.FirstOrDefault(a => a.ID == appId);
            if (app == null) return;
            app.Status = "Rejected";
            Core.Context.SaveChanges();
            LoadData();
        }
        #endregion

        #region Заявки на разморозку
        /// <summary>Одобряет заявку на разморозку пользователя или книги.</summary>
        private void ExecuteApproveUnfreeze(int appId)
        {
            var app = UnfreezeAppsList.FirstOrDefault(a => a.ID == appId);
            if (app == null) return;

            if (app.TargetTypeId == 1) // Пользователь
            {
                var target = Core.Context.Users.Find(app.UserId);
                if (target != null) { target.IsFrozen = false; app.Status = "Approved"; }
            }
            else if (app.TargetTypeId == 2) // Книга
            {
                var target = Core.Context.Books.Find(app.TargetBookId);
                if (target != null) { target.IsFrozen = false; app.Status = "Approved"; }
            }
            Core.Context.SaveChanges();
            MessageBox.Show("Заявка одобрена.");
            LoadData();
        }

        /// <summary>Отклоняет заявку на разморозку.</summary>
        private void ExecuteRejectUnfreeze(int appId)
        {
            var app = UnfreezeAppsList.FirstOrDefault(a => a.ID == appId);
            if (app == null) return;
            app.Status = "Rejected";
            Core.Context.SaveChanges();
            LoadData();
        }
        #endregion

        #region Управление замороженными объектами
        /// <summary>Размораживает книгу по её идентификатору.</summary>
        private void ExecuteUnfreezeBook(int bookId)
        {
            var book = Core.Context.Books.Find(bookId);
            if (book != null) { book.IsFrozen = false; Core.Context.SaveChanges(); MessageBox.Show("Книга разморожена."); LoadData(); }
        }

        /// <summary>Размораживает аккаунт пользователя по его идентификатору.</summary>
        private void ExecuteUnfreezeUser(int userId)
        {
            var user = Core.Context.Users.Find(userId);
            if (user != null) { user.IsFrozen = false; Core.Context.SaveChanges(); MessageBox.Show("Пользователь разморожен."); LoadData(); }
        }
        #endregion

        #region Обработка жалоб
        /// <summary>
        /// Обрабатывает жалобу: замораживает нарушителя (книгу/отзыв/автора) и удаляет жалобу из списка.
        /// </summary>
        /// <param name="complaintId">Идентификатор обрабатываемой жалобы.</param>
        private void ExecuteHandleComplaint(int complaintId)
        {
            var complaint = Core.Context.Complaints.FirstOrDefault(c => c.ID == complaintId);
            if (complaint == null) return;

            string message = "";

            try
            {
                if (complaint.ReviewId.HasValue)
                {
                    var review = Core.Context.Reviews.Find(complaint.ReviewId.Value);
                    if (review != null)
                    {
                        var author = Core.Context.Users.Find(review.UserId);
                        if (author != null)
                        {
                            author.IsFrozen = true;
                            Core.Context.Reviews.Remove(review);
                            message = $"Отзыв удалён. Автор отзыва ({author.Nickname}) заморожен.";
                        }
                    }
                }
                else if (complaint.BookId.HasValue)
                {
                    var book = Core.Context.Books.Find(complaint.BookId.Value);
                    if (book != null)
                    {
                        book.IsFrozen = true;
                        message = $"Книга \"{book.BookName}\" заморожена.";
                    }
                }
                else if (complaint.AuthorId.HasValue)
                {
                    var author = Core.Context.Users.Find(complaint.AuthorId.Value);
                    if (author != null)
                    {
                        author.IsFrozen = true;
                        message = $"Автор ({author.Nickname}) заморожен по жалобе.";
                    }
                }
                else
                {
                    message = "Жалоба обработана.";
                }

                Core.Context.Complaints.Remove(complaint);
                Core.Context.SaveChanges();

                MessageBox.Show(message, "Действие выполнено", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке жалобы: {ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}