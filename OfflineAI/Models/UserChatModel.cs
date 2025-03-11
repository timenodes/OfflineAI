using OfflineAI.Services;

namespace OfflineAI.Models
{
    /// <summary>
    /// 用户聊天
    /// </summary>
    public class UserChatModel : PropertyChangedBase
    {
        private bool _isAutoScrolling = false;      //是否自动滚动
        private bool _isHintVisible = true;         //提示是否可见
        private string _inputText = string.Empty;   //输入文本
        private string _directory = string.Empty;   //目录
        private string _fileName = string.Empty;    //文件
        private string _submitButtonName = "提交";  //提交按钮名称
        private OllamaService _ollama;              //共享Ollama对象
        private bool _isShowRunState = true;        //是否显示运行状态
        private bool? _runState = false;            //运行状态
        /// <summary>
        /// 是否自动滚动
        /// </summary>
        public bool IsAutoScrolling {
            get => _isAutoScrolling;
            set
            {
                if (_isAutoScrolling != value)
                {
                    _isAutoScrolling = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 是否显示运行状态
        /// </summary>
        public bool IsShowRunState
        {
            get => _isShowRunState;
            set
            {
                if (_isShowRunState != value)
                {
                    _isShowRunState = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 输入文本
        /// </summary>
        public string InputText {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 运行状态
        /// </summary>
        public bool? RunState
        {
            get => _runState;
            set
            {
                if (_runState != value)
                {
                    _runState = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 是否显示提示
        /// </summary>
        public bool IsHintVisible
        {
            get => _isHintVisible;
            set
            {
                if (_isHintVisible != value)
                {
                    _isHintVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 目录
        /// </summary>
        public string Directory {
            get => _directory;
            set
            {
                if (_directory != value)
                {
                    _directory = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 提交按钮名称
        /// </summary>
        public string SubmitButtonName {
            get => _submitButtonName;
            set
            {
                if (_submitButtonName != value)
                {
                    _submitButtonName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 共享Ollama对象 
        /// </summary>
        public OllamaService Ollama
        {
            get => _ollama;
            set
            {
                if (_ollama != value)
                {
                    _ollama = value;
                    RunState = _ollama.Connected;
                    OnPropertyChanged();
                }
            }
        }

    }
}
