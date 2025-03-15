using OfflineAI.Views;
using OfflineAI.Models;
using OfflineAI.Services;
using OfflineAI.Commands;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace OfflineAI.ViewModels
{
    /// <summary>
    /// 主窗体视图模型：
    /// 作者：吾与谁归
    /// 时间：2025年02月17日（首次创建时间）
    /// 功能: 
    /// 版本version: 1.0
    ///     1、2025-02-17：添加折叠栏展开|折叠功能。
    ///     2、2025-02-17：添加视图切换功能 1）系统设置 2) 聊天
    ///     3、2025-02-18：添加窗体关闭时提示。
    ///     4、2025-02-19：添加首页功能、修改新聊天功能。点击首页显示当前聊天，点击新聊天会创建新的会话（Chat）。
    ///     5、2025-02-20：添加窗体加载时传递Ollama对象功能。
    ///     6、2025-02-24：添加窗体加载时，加载聊天记录的功能。
    ///     7、2025-02-28：修复创建新对话后，无法查看记录的问题。
    /// 版本version: 1.1
    ///     1、2025-03-01：优化了类结构，创建对应的Model(MainWindowModel),将所有字段、属性移到Model。
    ///     2、2025-03-01：新增聊天记录窗体，修改了窗体加载时，加载聊天记录的功能。将其移动到了ChatRecordListView，在其视图模型中实现。
    ///     3、2025-03-10：移除了折叠栏功能，更新为Grid区域的显示与隐藏
    ///     4、2025-03-14：新增基础配置加载功能。
    /// </summary>
    public class MainViewModel : PropertyChangedBase
    {
        #region 字段、属性、命令

        #region 字段

        /// <summary>
        /// 主窗体模型对象
        /// </summary>
        private MainWindowModel _mainModel = new MainWindowModel();

        #endregion

        #region 属性
        /// <summary>
        ///  获取聊天记录视图对象
        /// </summary>
        private ChatRecordListView? GetChatRecordView
        {
            get => MainModel.ExpandedBarView as ChatRecordListView;
        }

        /// <summary>
        /// 主窗体模型对象
        /// </summary>
        public MainWindowModel MainModel 
        { 
            get => _mainModel;
            set
            {
                if (_mainModel != value)
                {
                    _mainModel = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region 命令属性

        /// <summary>
        /// 折叠功能菜单命令
        /// </summary>
        public ICommand ExpandedMenuCommand { get; private set; }
        /// <summary>
        /// 切换视图命令
        /// </summary>
        public ICommand SwitchViewCommand { get; private set; }
        /// <summary>
        /// 窗体关闭命令
        /// </summary>
        public ICommand ClosingWindowCommand {  get; private set; }
        /// <summary>
        /// 窗体加载命令
        /// </summary>
        public ICommand LoadedWindowCommand { get; private set; }
       
        #endregion

        #endregion

        #region 构造函数
        public MainViewModel()
        {
            Initialize();
        }
        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Initialize()
        {
            //初始化Ollama
            MainModel.Ollama = new OllamaService();
            MainModel.ModelListCollection = MainModel.Ollama.ModelList;
            MainModel.SelectedModel = MainModel.Ollama.SelectModel;

            //创建命令
            SwitchViewCommand = new ParameterCommand(OnSwitchView);
            LoadedWindowCommand = new EventsCommand<object>(OnLoadedWindow);
            ClosingWindowCommand = new EventsCommand<object>(OnClosingWindow);
            ExpandedMenuCommand = new EventsCommand<object>(OnExpandedMenu);
            //初始化视图集合
            MainModel.ViewCollection = new ObservableCollection<UserControl>();
            //添加视图到集合
            MainModel.ViewCollection.Add(new SystemSettingView());
            MainModel.ViewCollection.Add(new UserChatView());
            //默认显示视图
            MainModel.CurrentView = MainModel.ViewCollection[1];
            //设置折叠栏显示视图
            MainModel.ExpandedBarView = new ChatRecordListView();
            //获取聊天视图对象 //注册事件回调
            GetChatRecordView.ViewModel.RegisterCallBack(GetUserControl<UserChatView>().ViewModel.LoadChatRecordCallback);
            //折叠栏折叠状态
            MainModel.ExpandedBarWidth = 0;
        }

        #endregion

        #region 命令方法

        /// <summary>
        /// 触发主视图窗体加载方法：窗体加载时传递Ollama对象
        /// </summary>
        private void OnLoadedWindow(object obj)
        {
            Debug.Print(obj?.ToString());
            var systemView = GetUserControl<SystemSettingView>();
            systemView.ViewModel.Model.Ollama = MainModel.Ollama;
            systemView.ViewModel.LoadData();
            MainModel.SelectedModel = systemView.ViewModel.Model.Ollama.SelectModel;
            var userView = GetUserControl<UserChatView>();
            userView.ViewModel.Model.Ollama = MainModel.Ollama;
        }
       
        /// <summary>
        /// 触发关闭窗体方法
        /// </summary>
        private void OnClosingWindow(object obj)
        {
            if (obj is CancelEventArgs cancelEventArgs)
            {
                if (MessageBox.Show("确定要关闭程序吗？", "确认关闭", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    cancelEventArgs.Cancel = true; // 取消关闭
                }
                else
                {
                    GetUserControl<SystemSettingView>().ViewModel.SaveConfigCommand.Execute(null);
                    ClearResources();
                }
            }
        }

        /// <summary>
        /// 视图切换命令触发的方法
        /// </summary>
        private void OnSwitchView(object obj)
        {
            Debug.WriteLine(obj.ToString());
            switch (obj.ToString())
            {
                case "SystemSettingView":
                    MainModel.CurrentView = MainModel.ViewCollection[0];
                    break;
                case "UserChatView":
                    MainModel.CurrentView = MainModel.ViewCollection[1];
                    break;
                case "NewUserChatView": //新建聊天窗体
                    NewChat();
                    break;
            }
        }
        /// <summary>
        /// 折叠菜单功能
        /// </summary>
        private void OnExpandedMenu(object obj)
        {
            if (MainModel.ExpandedMenuIsHide == Visibility.Visible)
            {
                MainModel.ExpandedMenuIsHide = Visibility.Hidden;
                MainModel.ExpandedBarWidth = 0;
            }
            else
            {
                MainModel.ExpandedMenuIsHide = Visibility.Visible;
                MainModel.ExpandedBarWidth = 250;
            }
        }
        #endregion

        #region 其他方法
        /// <summary>
        /// 获取用户控件视图：获取集合中的视图对象
        /// </summary>
        public T? GetUserControl<T>() where T : UserControl
        {
            return MainModel.ViewCollection.FirstOrDefault(obj => obj is T) as T;
        }

        /// <summary>
        /// 释放资源：窗体关闭时触发
        /// </summary>
        private void ClearResources()
        {
        }

        /// <summary>
        /// 新建聊天窗体
        /// </summary>
        private void NewChat()
        {
            var view = GetUserControl<UserChatView>();
            UserChatView newChatView = new UserChatView();
            //取消注册的回调
            GetChatRecordView.ViewModel.UnRegisterCallBack(view.ViewModel.LoadChatRecordCallback);
            //给当前对象注册回调
            GetChatRecordView.ViewModel.RegisterCallBack(newChatView.ViewModel.LoadChatRecordCallback);
            //创建新的Chat对象并初始化数据
            MainModel.Ollama.CreateNewChat();
            newChatView.ViewModel.Model.Ollama = MainModel.Ollama;
            MainModel.ViewCollection[1] = null;
            MainModel.ViewCollection[1] = newChatView;
            MainModel.CurrentView = newChatView;
        }
        #endregion
    }
}
