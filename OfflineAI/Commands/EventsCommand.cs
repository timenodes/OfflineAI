using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

/// <summary>
/// 事件命令：
///   有些控件的无法绑定命令，但是想要实现命令绑定功能，可通过创建该命令实现。
///   需要引用Microsoft.Xaml.Behaviors.Wpf组合实现。
/// </summary>
namespace OfflineAI.Commands
{
    public class EventsCommand<T> : ICommand
    {
        private readonly Action<T> _execute;

        private readonly Func<T, bool> _canExecute;
        public EventsCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    /// <summary>
    /// 事件参数转换器
    /// </summary>
    public class MouseDownConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is object dataItem && values[1] is MouseButtonEventArgs eventArgs)
            {
                return new MouseDownParameters
                {
                    DataItem = dataItem,
                    EventArgs = eventArgs
                };
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MouseDownParameters
    {
        public object DataItem { get; set; } // 当前绑定的数据项
        public MouseButtonEventArgs EventArgs { get; set; } // 鼠标事件参数
    }
}