using OfflineAI.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace OfflineAI.Models
{
    /// <summary>
    /// 主窗体:
    ///     1、折叠栏宽度
    ///     2、选择的模型
    ///     3、Ollama服务对象
    ///     4、当前视图
    ///     5、折叠栏视图
    ///     6、模型列表
    /// </summary>
    public class MainWindowModel: PropertyChangedBase
    {
        private int _expandedBarWidth = 50;     //折叠栏宽度
        private string _selectedModel;          //选择的模型
        private Visibility _expandedMenuIsHide = Visibility.Hidden;//折叠栏是否隐藏

        private OllamaService _ollamaService;                  // Ollama服务对象
        private UserControl _currentView;                           // 当前显示视图
        private UserControl _expandedBarView;                       // 折叠栏视图
        private ObservableCollection<string> _modelListCollection;  // 模型列表集合

        /// <summary>
        /// 视图集合，保存视图
        /// </summary>
        public ObservableCollection<UserControl> ViewCollection { get; set; }

        /// <summary>
        /// 折叠栏宽度
        /// </summary>
        public int ExpandedBarWidth
        {
            get => _expandedBarWidth;
            set
            {
                if (value != _expandedBarWidth)
                {
                    _expandedBarWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 选择的模型
        /// </summary>
        public string SelectedModel 
        { 
            get => _selectedModel; 
            set 
            {
                if (value != _selectedModel)
                {
                    _selectedModel = value;
                    _ollamaService.SelectModel = _selectedModel;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility ExpandedMenuIsHide { 
            get => _expandedMenuIsHide; 
            set
            {
                if (value != _expandedMenuIsHide)
                {
                    _expandedMenuIsHide = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Ollama服务对象
        /// </summary>
        public OllamaService Ollama
        {
            get => _ollamaService;
            set => _ollamaService = value;
        }
        
        /// <summary>
        /// 当前显示视图
        /// </summary>
        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }
         
        /// <summary>
        /// 当前折叠栏视图
        /// </summary>
        public UserControl ExpandedBarView
        {
            get => _expandedBarView;
            set
            {
                if (_expandedBarView != value)
                {
                    _expandedBarView = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 模型列表集合
        /// </summary>
        public ObservableCollection<string> ModelListCollection
        {
            get => _modelListCollection;
            set
            {
                if (_modelListCollection != value)
                {
                    _modelListCollection = value;
                    OnPropertyChanged();
                }
            }
        }

       
    }
}
