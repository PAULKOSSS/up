using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    public partial class AdminView : UserControl
    {
        public AdminView()
        {
            InitializeComponent();
            this.DataContext = new AdminViewModel();
        }
    }
}