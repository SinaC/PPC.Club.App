namespace PPC.Services
{
    public class IOService : IIOService
    {
        public string OpenFileDialog(string defaultPath)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = defaultPath,
                DefaultExt = ".csv",
                Filter = "CSV documents (.csv)|*.csv"
            };
            if (dialog.ShowDialog() == true)
                return dialog.FileName;
            return null;
        }
    }
}
