namespace up.Infrastructure
{
    /// <summary>
    /// Статический класс-синглтон для хранения данных текущей авторизованной сессии пользователя.
    /// Данные доступны глобально в рамках жизненного цикла приложения.
    /// </summary>
    public static class UserSession
    {
        public static int UserId { get; set; }

        public static string Login { get; set; }

        public static string RoleName { get; set; }

        public static bool IsFrozen { get; set; }

        public static bool IsAdmin => RoleName == "Администратор";

        public static bool IsAuthor => RoleName == "Автор";

        public static bool IsReader => RoleName == "Читатель";
    }
}