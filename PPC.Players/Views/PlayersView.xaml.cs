using System.Windows.Controls;

namespace PPC.Players.Views
{
    /// <summary>
    /// Interaction logic for PlayersView.xaml
    /// </summary>
    public partial class PlayersView : UserControl
    {
        public PlayersView()
        {
            InitializeComponent();
        }

        private void DataGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if ((string)e.Column.Header == "DCI" && !e.Row.IsNewItem)
                    e.Cancel = true;
        }
    }
}
