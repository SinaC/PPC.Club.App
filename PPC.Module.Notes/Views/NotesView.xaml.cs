using System.Windows;
using System.Windows.Controls;

namespace PPC.Module.Notes.Views
{
    /// <summary>
    /// Interaction logic for NotesView.xaml
    /// </summary>
    public partial class NotesView : UserControl
    {
        public NotesView()
        {
            InitializeComponent();
        }

        private void NoteTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null)
                return;
            tb.CaretIndex = tb.Text.Length;
            tb.ScrollToEnd();
        }
    }
}
