namespace PPC.Services.IO
{
    public class IOService : IIOService
    {
        public string OpenFileDialog(string defaultPath, string defaultExt, string filter)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = defaultPath,
                DefaultExt = defaultExt,
                Filter = filter
            };
            if (dialog.ShowDialog() == true)
                return dialog.FileName;
            return null;
        }
    }
}
