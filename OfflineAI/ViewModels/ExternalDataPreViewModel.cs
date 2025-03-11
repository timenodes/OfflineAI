using OfflineAI.Models;

namespace OfflineAI.ViewModels
{
    /// <summary>
    /// 外部数据预览视图模型：
    /// 描述：聊天记录列表视图模型
    /// 作者：吾与谁归
    /// 时间：2025年3月7日
    /// 功能：
    /// 版本version: 1.1
    ///    1、 根据文件类型设置预览图标。
    /// </summary>
    public class ExternalDataPreViewModel:PropertyChangedBase
    {

        public ExternalDataPreModel _model = new ExternalDataPreModel();
        public ExternalDataPreModel Model 
        { 
            get =>_model;  
            set
            {
                if (_model == value)
                {
                    _model = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ExternalDataPreViewModel()
        {
            Model.ImageSource = "/Resources/Images/text-file-blue-64.png";
        }

        /// <summary>
        /// 根据文件类型设置图像源
        /// </summary>
        public void SetImageSource(ExternalDataType fileType)
        {
            switch (fileType)
            {
                case ExternalDataType.Text:
                    Model.ImageSource = "/Resources/Images/text-file-blue-64.png";
                    break;
                case ExternalDataType.Image:
                    Model.ImageSource = "/Resources/Images/image-file-blue-64.png";
                    break;
                default:
                    Model.ImageSource = "/Resources/Images/unknown-red-64.png";
                    break;
            }

            
        }
    }
}
