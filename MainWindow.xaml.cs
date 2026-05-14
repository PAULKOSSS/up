using System.Windows;
using up.Infrastructure;

namespace up
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigationHelper.ContentFrame = MainFrame;
        }
    }
}