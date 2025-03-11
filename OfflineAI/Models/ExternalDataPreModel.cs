namespace OfflineAI.Models
{
    /// <summary>
    /// 外部数据预览模型
    /// </summary>
    public class ExternalDataPreModel:PropertyChangedBase
    {
        /// <summary>
        /// 数据索引
        /// </summary>
        private object _index;
        /// <summary>
        /// 数据索引
        /// </summary>
        public object Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 外部文件路径名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public ExternalDataType DataType
        {
            get; set;
        }

        /// <summary>
        /// 图像源
        /// </summary>
        private string imageSource;
        /// <summary>
        /// 图像源
        /// </summary>
        public string ImageSource
        {
            get => imageSource;
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// 外部数据类型
    /// </summary>
    public enum ExternalDataType
    {
        /// <summary>
        /// 文本类型
        /// </summary>
        Text,
        /// <summary>
        /// 图像类型
        /// </summary>
        Image,
        /// <summary>
        /// 未知类型（其他类型）
        /// </summary>
        Unknown
    }
}
