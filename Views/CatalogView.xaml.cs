using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    public partial class CatalogView : UserControl
    {
        public CatalogView()
        {
            InitializeComponent();
            this.DataContext = new CatalogViewModel();
        }
    }
}