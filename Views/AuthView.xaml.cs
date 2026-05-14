using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    // ✅ Изменено на UserControl
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
            this.DataContext = new AuthViewModel();
        }
    }
}