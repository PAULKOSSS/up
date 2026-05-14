using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
            this.DataContext = new ProfileViewModel();
        }
    }
}