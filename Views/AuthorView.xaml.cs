using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    public partial class AuthorView : UserControl
    {
        public AuthorView()
        {
            InitializeComponent();
            this.DataContext = new AuthorViewModel();
        }
    }
}