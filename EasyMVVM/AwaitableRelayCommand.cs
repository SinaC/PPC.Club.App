using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyMVVM
{
    public class AwaitableRelayCommand : AwaitableRelayCommand<object>, IAsyncCommand
    {
        public AwaitableRelayCommand(Func<Task> executeMethod)
            : base(o => executeMethod())
        {
        }

        public AwaitableRelayCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
        }
    }

    public class AwaitableRelayCommand<T> : IAsyncCommand<T>
    {
        private readonly Func<T, Task> _executeMethod;
        private readonly RelayCommand<T> _underlyingCommand;
        private bool _isExecuting;

        public AwaitableRelayCommand(Func<T, Task> executeMethod)
            : this(executeMethod, _ => true)
        {
        }

        public AwaitableRelayCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _executeMethod = executeMethod;
            _underlyingCommand = new RelayCommand<T>(x => { }, canExecuteMethod);
        }

        public async Task ExecuteAsync(T obj)
        {
            try
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();
                await _executeMethod(obj);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && _underlyingCommand.CanExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { _underlyingCommand.CanExecuteChanged += value; }
            remove { _underlyingCommand.CanExecuteChanged -= value; }
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }
    }

}
