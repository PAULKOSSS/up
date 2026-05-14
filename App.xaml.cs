using System.Windows;
using up.Infrastructure;

namespace up
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Всегда начинаем с авторизации
            var mainWindow = new MainWindow();
            mainWindow.Show();
            NavigationHelper.Navigate(new Views.AuthView());
        }
    }
}