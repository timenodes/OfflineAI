using OllamaSharp;
using System.Collections.ObjectModel;

namespace OfflineAI.Services
{
    /// <summary>
    /// 共享Ollama对象类：保持Ollama对象一致才能使用当前对象实现对话
    /// 作    者：吾与谁归
    /// 时    间：2025年02月18日
    /// 功    能：
    ///     1） 2025-02-18：使用cmd命令启动Ollama服务,目前使用ollama list();
    ///     2） 2025-02-18：初始化模型参数，在初始化时启用GPU、连接ollama、初始化模型。
    /// </summary>
    public class OllamaService
    {
        #region 字段|属性

        #region 字段
        private bool _connected = false;        //连接状态
        private Chat chat;                      //构建交互式聊天模型对象。
        private OllamaApiClient _ollama;        //OllamaAPI对象
        private string _selectModel;            //选择的模型名称
        #endregion

        #region 属性
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected
        {
            get { return _connected; }
            set { _connected = value; }
        }

        /// <summary>
        /// 选择的模型名称
        /// </summary>
        public string SelectModel { 
            get => _selectModel;
            set
            {
                if(value != _selectModel)
                {
                    _selectModel = value;
                    _ollama.SelectedModel = value;
                    chat.Model = value;
                }
            }
        }

        /// <summary>
        /// 构建交互式聊天模型对象。
        /// </summary>
        public Chat Chat
        {
            get { return chat; }
            set { chat = value; }
        }

        /// <summary>
        /// OllamaAPI对象
        /// </summary>
        public OllamaApiClient Ollama
        {
            get { return _ollama; }
            set { _ollama = value; }
        }

        /// <summary>
        /// 模型列表
        /// </summary>
        public ObservableCollection<string> ModelList { get; set; }
        #endregion

        #endregion

        #region 构造函数
        public OllamaService()
        {
            ProcessService.ExecuteCommand("ollama list");
            Initialize("llama3.2:3b");
        }
        #endregion

        #region 其他方法
        /// <summary>
        /// 初始化方法
        /// </summary>
        private void Initialize( string modelName)
        {
            try
            {
                // 设置默认设备为GPU
                Environment.SetEnvironmentVariable("OLLAMA_DEFAULT_DEVICE", "gpu");
                //连接Ollama，并设置初始模型
                Ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
                //获取本地可用的模型列表
                ModelList = (ObservableCollection<string>)GetModelList();
                //遍历查找是否包含llama3.2:3b模型
                var tmepModelName = ModelList.FirstOrDefault(name => name.ToLower().Contains("llama3.2:3b"));
                //设置的模型不为空
                if (tmepModelName != null)
                {
                    Ollama.SelectedModel = tmepModelName;
                }
                //模型列表不为空
                else if (ModelList.Count > 0)
                {
                    _ollama.SelectedModel = ModelList[ModelList.Count - 1];
                }
                //Ollama服务启用成功
                chat = new Chat(_ollama);
                SelectModel = _ollama.SelectedModel;
                _connected = true;
            }
            catch (Exception)
            {
                _connected = false;     //Ollama服务启用失败
            }
        }

        /// <summary>
        /// 获取模型里列表
        /// </summary>
        public Collection<string> GetModelList()
        {
            var models = _ollama.ListLocalModelsAsync();
            
            var modelList = new ObservableCollection<string>();
            foreach (var model in models.Result)
            {
                modelList.Add(model.Name);
            }
            return modelList;
        }

        /// <summary>
        /// 创建新的聊天对象
        /// </summary>
        public void CreateNewChat()
        {
            try
            {
                var version = _ollama.GetVersionAsync();
                if (version?.Result != null)
                {
                    chat = new Chat(_ollama);
                    Connected = true;
                }
                else
                {
                    chat = new Chat(_ollama);
                    Connected = false;
                }
            }
            catch (Exception ex)
            {
                Connected = false;
            }
            
        }
        #endregion

    }
}
