using System.Windows.Controls;
using up.ViewModels;

namespace up.Views
{
    public partial class BookEditorView : UserControl
    {
        public BookEditorView()
        {
            InitializeComponent();
        }

        public BookEditorView(BookEditorViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}