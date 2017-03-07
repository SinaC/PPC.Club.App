namespace PPC.Services
{
    public interface IIOService
    {
        string OpenFileDialog(string defaultPath, string defaultExt, string filter);
    }
}
