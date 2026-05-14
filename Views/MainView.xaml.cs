using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    // ✅ Изменено на UserControl
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}