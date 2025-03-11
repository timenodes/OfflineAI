
using System.Windows.Input;

namespace OfflineAI.Commands
{
    /// <summary>
    /// 参数命令：
    ///     可以带参数的命令：
    /// </summary>
    public class ParameterCommand : ICommand
    {
        public Action<object> execute;
        public ParameterCommand(Action<object> execute)
        {
            this.execute = execute;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter)
        {
            return CanExecuteChanged != null;
        }
        public void Execute(object? parameter)
        {
            execute?.Invoke(parameter);
        }
    }
}
