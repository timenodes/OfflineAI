using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OfflineAI.Models
{
    /// <summary>
    /// 属性变更基类
    /// </summary>
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
