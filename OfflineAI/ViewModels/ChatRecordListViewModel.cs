using OfflineAI.Models;
using OfflineAI.Services;
using OfflineAI.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace OfflineAI.ViewModels
{
    /// <summary>
    /// 描述：聊天记录列表视图模型
    /// 作者：吾与谁归
    /// 时间：2025年3月1日
    /// 功能：
    /// 版本version: 1.1
    ///     2025-03-01：将聊天记录列表从主窗体中分离（MainWindow --> ChatRecordListView）。
    ///     2025-03-05：更新记录文件加载功能，显示提问日期。
    ///     2025-03-06：新增功能，新聊天后第一次提问完成后，保存的记录刷新到记录列表。
    ///     2025-03-07：新增聊天记录删除功能。
    /// </summary>
    public class ChatRecordListViewModel: PropertyChangedBase
    {
        
        #region 字段、属性

        #region 字段
        /// <summary>
        /// 聊天记录集合：
        /// </summary>
        private ObservableCollection<ChatDataModel> _chatRecordCollection;
        /// <summary>
        /// 文件侦听
        /// </summary>
        private FileSystemWatcher FileWatcher = new FileSystemWatcher();
        #endregion

        #region 属性
        /// <summary>
        /// 聊天记录集合
        /// </summary>
        public ObservableCollection<ChatDataModel> ChatRecordCollection
        {
            get => _chatRecordCollection;
            set
            {
                if (_chatRecordCollection != value)
                {
                    _chatRecordCollection = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 事件：加载聊天记录
        /// </summary>
        public event Action<string> LoadChatRecordEventHandler;

        /// <summary>
        /// 聊天记录鼠标点击事件
        /// </summary>
        public ICommand ChatRecordMouseDownCommand { get; set; }
        #endregion

        #endregion

        #region 构造函数、初始化方法
        public ChatRecordListViewModel()
        {
            Initialize();
        }
        ~ChatRecordListViewModel()
        {
            StopFileWatcher();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            ChatRecordMouseDownCommand = new RelayCommand(OnChatRecordMouseDown);
            LoadChatRecord();
            StartFileWatcher($"{AppDomain.CurrentDomain.BaseDirectory}\\Record");
        }
       
        /// <summary>
        /// 菜单项鼠标按下:删除文件
        /// </summary>
        private void OnMenuItemMouseDown(object obj)
        {
            Debug.Print("右键菜单");
            if(obj is string uri)
            {
                File.Delete(uri);
                Debug.Print($"删除记录：{uri}");
            }
        }
        #endregion

        #region 命令方法
        /// <summary>
        /// 事件：聊天记录鼠标按下:
        ///     版本1：传递ChatDataModel 对象，只回调URI。
        ///     版本2：传递MouseButtonEventArgs对象，路径设置在TextBlock的Tag中，并判定鼠标是左键还是右键，进行其他操作。
        /// 设想：像事件一样,传递（obeject sender ,MouseButtonEventArgs e）,使用多路绑定MultiBinding结合转换器Converter，但是未能实现。
        /// </summary>
        private void OnChatRecordMouseDown(object args)
        {
            if (args is MouseButtonEventArgs mouseDown)
            {
                //鼠标左键
                if (mouseDown.ChangedButton == MouseButton.Left)
                {
                    if (mouseDown.Source is TextBlock textBlock)
                    {
                        Debug.Print(textBlock.Tag.ToString());
                        OnLoadChatRecordCallBack(textBlock.Tag.ToString());
                    }
                }
                //鼠标右键
                if (mouseDown.ChangedButton == MouseButton.Right)
                {
                    if (mouseDown.Source is TextBlock textBlock)
                    {
                        Debug.Print(textBlock.Tag.ToString());
                    }
                }
            }
        }
        #endregion

        #region 其他方法
        /// <summary>
        /// 加载聊天记录:
        /// </summary>
        private void LoadChatRecord()
        {
            string directory = $"{AppDomain.CurrentDomain.BaseDirectory}\\Record";
            ObservableCollection<ChatDataModel> records = new ObservableCollection<ChatDataModel>();
            ChatRecordCollection = new ObservableCollection<ChatDataModel>();
            string[] files = DataService.GetFiles(directory);
            foreach (string file in files)
            { 
                ///读取Json文件中的数据
                List<ChatJsonDataModel>? datas = DataService.ReadDataFormJsonFile(file);
                if (datas != null && datas[0].Content.Trim().Length > 0)
                {
                    ChatDataModel dataModel = new ChatDataModel();
                    dataModel.JsonModel = datas[0];     //Json数据
                    dataModel.Uri = file;               //加载链接           
                    dataModel.MenuItemMouseDownCommand = new RelayCommand(OnMenuItemMouseDown);
                    records.Add(dataModel);
                    Debug.WriteLine(datas[0].Content);
                }
            }
            var sortDatas = records.OrderByDescending(e => DateTime.Parse(e.JsonModel.Date)).ToList();
            foreach (ChatDataModel dataModel in sortDatas)
            {
                ChatRecordCollection.Add(dataModel);
                Debug.Print($"{dataModel.JsonModel.Date}");
            }
            //ChatRecordCollection = records;
        }

        /// <summary>
        /// 开始文件侦听
        /// </summary>
        private void StartFileWatcher(string directory)
        {
            FileWatcher.Path = directory;
            FileWatcher.Filter = "*.json";
            FileWatcher.IncludeSubdirectories = true;
            FileWatcher.Created += OnCreated;       // 侦听文件创建事件
            FileWatcher.Deleted += OnDeleted;       // 侦听文件删除事件
            FileWatcher.EnableRaisingEvents = true; // 开始侦听
            Debug.Print("开始侦听目录: " + directory);
            Debug.Print("按任意键退出...");
        }
        /// <summary>
        /// 停止文件侦听
        /// </summary>
        private void StopFileWatcher()
        {
            FileWatcher.EnableRaisingEvents = false; // 停止侦听
            Debug.Print("停止侦听目录: " + FileWatcher.Path);
        }
        #endregion

        #region 事件方法
        /// <summary>
        /// 触发回调：加载聊天记录：
        /// 1、发布通知：将文件名发布给订阅者，订阅者收到通知后，执行加载聊天记录操作
        /// </summary>
        private void OnLoadChatRecordCallBack(string args)
        {
            LoadChatRecordEventHandler?.Invoke(args);
        }

        /// <summary>
        /// 注册回调：
        /// 1、LoadChatRecordEventHandler事件
        /// </summary>
        public void RegisterCallBack(Action<string> action)
        {
            LoadChatRecordEventHandler += action;
        }

        /// <summary>
        /// 取消注册回调
        /// 1、LoadChatRecordEventHandler事件
        /// </summary>
        public void UnRegisterCallBack(Action<string> action)
        {
            LoadChatRecordEventHandler -= action;
        }

        /// <summary>
        /// 文件创建事件处理程序：
        ///     新建聊天记录，第一次回答后新增文件，此时触发文件变更事件，将新记录添加到记录集合。
        /// </summary>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Debug.Print($"文件已创建: {e.FullPath}");
            OnFileChanged(e.FullPath, FileChangeType.Created);
        }
        /// <summary>
        /// 事件：文件删除事件
        ///     删除聊天记录，删除文件时，会触发文件删除事件，将记录从记录集合中移除。
        /// </summary>
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Debug.Print($"文件已删除: {e.FullPath}");
            OnFileChanged(e.FullPath, FileChangeType.Deleted);
        }
        /// <summary>
        /// 事件：触发文件变更事件。
        /// </summary>
        private void OnFileChanged(string fileName, FileChangeType options)
        {
            if (options == FileChangeType.Created)
            {
                List<ChatJsonDataModel>? datas = DataService.ReadDataFormJsonFile(fileName);
                ///读取Json文件中的数据
                if (datas != null && datas[0].Content.Trim().Length > 0)
                {
                    ChatDataModel dataModel = new ChatDataModel();
                    dataModel.JsonModel = datas[0];
                    dataModel.Uri = fileName;
                    //使用线程异步删除
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ChatRecordCollection.Insert(0,dataModel);
                    }));
                    Debug.WriteLine(datas[0].Content);
                }
            }
            //执行移除操作
            if (options == FileChangeType.Deleted)
            {
                //使用线程异步删除
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ChatRecordCollection.Remove(ChatRecordCollection.FirstOrDefault(obj => obj.Uri.Equals(fileName)));
                }));
               
            }
        }
        #endregion
    }
}
