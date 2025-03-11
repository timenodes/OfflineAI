using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OfflineAI.ExtensionTool
{
    /// <summary>
    /// 限定大小集合
    /// </summary>
    public class LimitedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly int _maxSize;

        public LimitedObservableCollection(int maxSize)
        {
            _maxSize = maxSize;
            CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Count > _maxSize)
            {
                // 移除超出部分的元素
                for (int i = Count - 1; i >= _maxSize; i--)
                {
                    RemoveAt(i);
                }
            }
        }
    }
}
