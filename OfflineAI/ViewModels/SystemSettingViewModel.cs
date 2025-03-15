using Microsoft.Win32;
using Newtonsoft.Json;
using OfflineAI.Commands;
using OfflineAI.Models;
using OllamaSharp.Models;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Windows.Input;

namespace OfflineAI.ViewModels
{
    /// <summary>
    /// 软件系统设置视图模型
    /// 作者：吾与谁归
    /// 时间: 2025年2月19日
    /// 功能：
    /// 版本version: 1.1
    ///     1、2025-03-14 添加系统配置文件保存功能
    ///     2、2025-03-14 添加系统配置文件读取功能
    /// </summary>
    public class SystemSettingViewModel : PropertyChangedBase
    {

        #region 字段、属性
        #region 字段
        private SystemSettingModel _model;
        #endregion
        public SystemSettingModel Model
        {
            get => _model;
            private set => _model = value;
        }
        #region 属性
        public ICommand SelectedCommand { get; private set; }
        public ICommand SaveConfigCommand { get; private set; }
        
        #endregion
        #endregion

        #region 构造函数
        public SystemSettingViewModel()
        {
            Initialize();
        }
        private void Initialize()
        {
            Model = new SystemSettingModel();
            SelectedCommand = new EventsCommand<object>(OnSelected);
            SaveConfigCommand = new ParameterlessCommand(OnSaveConfig);
        }
        #endregion

        #region  命令方法
        private void OnSelected(object obj)
        {
            string initDirectory = Model.Setting.DataPath;
            OpenFolderDialog OpenFolder = new OpenFolderDialog();
            OpenFolder.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var flag = OpenFolder.ShowDialog();
            
            if (flag!=null && flag ==true)
            {
                Model.DataPath = OpenFolder.FolderName;
            }
        }
        
        public void LoadData()
        {
            var json = File.ReadAllText(Model.ConfigFile);
            ConfigParameterModel config = JsonConvert.DeserializeObject<ConfigParameterModel>(json);
            Model.Ollama.SelectModel = config.ModelName;
            Model.DataPath = config.DataPath;
        }
        private void OnSaveConfig()
        {
           Model.Setting.ModelName = Model.Ollama.SelectModel;
           string config = JsonConvert.SerializeObject(Model.Setting, Formatting.Indented);
           if (!Directory.Exists(Path.GetDirectoryName(Model.ConfigFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Model.ConfigFile));
            }
            File.WriteAllText(Model.ConfigFile, config);
        }
        #endregion
    }
}
