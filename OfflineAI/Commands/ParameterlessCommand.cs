
using System.Windows.Input;

namespace OfflineAI.Commands
{
    /// <summary>
    /// 无参数命令：
    ///     无参数的命令：
    /// </summary>
    public class ParameterlessCommand : ICommand
    {
        private Action _execute;
        public ParameterlessCommand(Action execute)
        {
            this._execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return CanExecuteChanged != null;
        }

        public void Execute(object? parameter)
        {
            _execute.Invoke();
        }
    }
}
