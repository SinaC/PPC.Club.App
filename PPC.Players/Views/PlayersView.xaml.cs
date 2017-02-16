using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPC.Players.Views
{
    /// <summary>
    /// Interaction logic for PlayersView.xaml
    /// </summary>
    public partial class PlayersView : UserControl, INotifyPropertyChanged
    {
        public PlayersView()
        {
            InitializeComponent();
        }

        private bool _isInEditMode;
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if (_isInEditMode != value)
                {
                    _isInEditMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private void DataGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if ((string)e.Column.Header == "DCI" && !e.Row.IsNewItem)
                    e.Cancel = true;
            else
                IsInEditMode = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            IsInEditMode = false;
        }

        private void FilterTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && PlayersDataGrid.SelectedItem != null)
            {
                if (PlayersDataGrid.Items.Count - 1 > PlayersDataGrid.SelectedIndex)
                    PlayersDataGrid.SelectedIndex++;
            }
            else if (e.Key == Key.Up && PlayersDataGrid.SelectedItem != null)
            {
                if (PlayersDataGrid.SelectedIndex > 0)
                    PlayersDataGrid.SelectedIndex--;
            }
        }
    }
}
