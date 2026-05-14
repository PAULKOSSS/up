using System;
using System.Linq;
using System.Windows;
using up.Infrastructure;
using up.Views;

namespace up.ViewModels
{
    /// <summary>
    /// ViewModel окна авторизации и регистрации пользователей.
    /// Управляет переключением режимов, валидацией данных и созданием сессии.
    /// </summary>
    public class AuthViewModel : ViewModelBase
    {
        private string _login;
        public string Login { get => _login; set { _login = value; OnPropertyChanged(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _nickname;
        public string Nickname { get => _nickname; set { _nickname = value; OnPropertyChanged(); } }

        private bool _isLoginMode = true;
        public bool IsLoginMode
        {
            get => _isLoginMode;
            set { _isLoginMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsRegisterMode)); }
        }
        public bool IsRegisterMode => !IsLoginMode;

        /// <summary>Команда выполнения входа в систему.</summary>
        public RelayCommand LoginCommand { get; }
        /// <summary>Команда выполнения регистрации нового пользователя.</summary>
        public RelayCommand RegisterCommand { get; }
        /// <summary>Команда переключения между режимами входа и регистрации.</summary>
        public RelayCommand SwitchModeCommand { get; }

        /// <summary>
        /// Инициализирует команды и устанавливает режим входа по умолчанию.
        /// </summary>
        public AuthViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            SwitchModeCommand = new RelayCommand(_ => IsLoginMode = !IsLoginMode);
        }

        /// <summary>
        /// Проверяет учётные данные, создаёт сессию пользователя и перенаправляет в главное окно.
        /// </summary>
        /// <param name="obj">Параметр команды (не используется).</param>
        private void ExecuteLogin(object obj)
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Заполните логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.Login == Login && u.Password == Password);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var role = Core.Context.Roles.FirstOrDefault(r => r.ID == user.RoleId);
            UserSession.UserId = user.ID;
            UserSession.Login = user.Login;
            UserSession.RoleName = role?.RoleName ?? "Читатель";
            UserSession.IsFrozen = user.IsFrozen == true;

            NavigationHelper.Navigate(new MainView());
        }

        /// <summary>
        /// Регистрирует нового пользователя, проверяет уникальность логина/email и назначает роль по умолчанию.
        /// </summary>
        /// <param name="obj">Параметр команды (не используется).</param>
        private void ExecuteRegister(object obj)
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Nickname))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Core.Context.Users.Any(u => u.Login == Login))
            {
                MessageBox.Show("Такой логин уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Core.Context.Users.Any(u => u.Email == Email))
            {
                MessageBox.Show("Этот Email уже зарегистрирован!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var defaultRole = Core.Context.Roles.FirstOrDefault(r => r.RoleName == "Читатель");
            int roleId = defaultRole?.ID ?? 1;

            var newUser = new Users
            {
                Login = Login,
                Password = Password,
                Email = Email,
                Nickname = Nickname,
                RoleId = roleId,
                Registration = DateTime.Now,
                IsFrozen = false
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();

            MessageBox.Show("Регистрация успешна! Теперь войдите.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            IsLoginMode = true; 
            Login = Password = Email = Nickname = ""; 
        }
    }
}