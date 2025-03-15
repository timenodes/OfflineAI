
using OfflineAI.Services;

namespace OfflineAI.Models
{
    public class SystemSettingModel: PropertyChangedBase
    {
        private ConfigParameterModel _setting;
        private OllamaService ollama;
        /// <summary>
        /// 数据保存路径
        /// </summary>
        public readonly string ConfigFile = AppDomain.CurrentDomain.BaseDirectory+"\\Config.json";
        /// <summary>
        /// 选择路径
        /// </summary>
        
        public string DataPath
        {
            get=>Setting.DataPath;
            set
            {
                if (Setting.DataPath != value)
                {
                    Setting.DataPath = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Ollama
        /// </summary>
        public OllamaService Ollama { 
            get => ollama; 
            set
            {
                if (ollama != value)
                {
                    ollama = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 配置参数
        /// </summary>
        public ConfigParameterModel Setting
        {
            get => _setting;
            set
            {
                if (_setting != value)
                {
                    _setting = value;
                    OnPropertyChanged();
                }
            }
        }

        public SystemSettingModel()
        {
            Setting = new ConfigParameterModel();
            Setting.DataPath = AppDomain.CurrentDomain.BaseDirectory+"\\Record";
        }
    }
}
