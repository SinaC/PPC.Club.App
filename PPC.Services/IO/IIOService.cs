namespace PPC.Services.IO
{
    public interface IIOService
    {
        string OpenFileDialog(string defaultPath, string defaultExt, string filter);
    }
}
