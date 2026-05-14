using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    public partial class BookListsView : UserControl
    {
        public BookListsView()
        {
            InitializeComponent();
            this.DataContext = new BookListsViewModel();
        }
    }
}