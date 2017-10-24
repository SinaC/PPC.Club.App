using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyMVVM
{
    public interface IAsyncCommand : IAsyncCommand<object>
    {
    }

    public interface IAsyncCommand<in T> : ICommand
    {
        Task ExecuteAsync(T obj);
    }

}
