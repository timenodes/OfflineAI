using System.Windows.Input;

namespace OfflineAI.Models
{
    /// <summary>
    /// 聊天数据：
    /// </summary>
    public class ChatDataModel: PropertyChangedBase
    {
        public ChatDataModel()
        {
            JsonModel = new ChatJsonDataModel();
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string? Uri { get; set; }

        /// <summary>
        /// Json数据模型
        /// </summary>
        public ChatJsonDataModel? JsonModel { get; set; }

        /// <summary>
        ///  菜单项鼠标按下命令
        /// </summary>
        public ICommand MenuItemMouseDownCommand { get; set; }
    }

    
}
