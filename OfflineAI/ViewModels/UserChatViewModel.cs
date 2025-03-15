using System.IO;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;

using Markdig;
using Markdig.Wpf;
using Markdig.Syntax;
using Microsoft.Win32;
using OllamaSharp.Models.Chat;

using OfflineAI.Models;
using OfflineAI.Commands;
using OfflineAI.Services;

namespace OfflineAI.ViewModels
{
    /// <summary>
    /// 描述：用户聊天视图模型：
    /// 作者：吾与谁归
    /// 时间: 2025年2月19日
    /// 功能：
    /// 版本version: 1.0
    ///    1、 2025-02-19：添加了AI聊天功能，输出问题及结果到UI,并使用Markdown相关的库做简单渲染。
    ///    2、 2025-02-20：优化了构造函数，使用无参构造，方便在设计器中直接绑定数据上下文（感觉）。
    ///    3、 2025-02-20：添加了滑动查看内容（自动滚动，鼠标滚动）。
    ///    4、 2025-02-24：添加了聊天记录保存功能。
    ///    5、 2025-02-24：添加了聊天记录加载功能，通过点击记录列表显示。
    /// 版本version: 1.1
    ///    1、 2025-03-01：添加了根据聊天记录回复的功能。
    ///    2、 2025-03-01：添加了UserChatViewModel对应Model,将字段、属性移到Model中，方便后续扩展。
    ///    3、 2025-03-05：新增读取外部数据回复问题功能，目前支持txt文件。
    ///    4、 2025-03-05：新增添加图片提问题功能，模型需要支持视觉（如：minicpm-v:latest）。
    /// </summary>
    public class UserChatViewModel:PropertyChangedBase
    {
        #region 字段、属性、命令

        #region 字段

        private MarkdownViewer _markdownViewer;                     //MarkdownViewer控件
        private ScrollViewer scrollViewer;                          //ScrollViewer滑动控件
        private WrapPanel wrapPanel;                                //水平排序容器
        private CancellationTokenSource? _cts_ChatThread;           //聊天异步线程：取消标记
        private CancellationTokenSource? _cts_MessageQueue;         //消息异步线程：取消标记
        private ConcurrentQueue<string> _messageQueue;              //消息异步线程：消息队列
        //自定义
        private DataService _userDataService;                       //聊天数据服务
        private UserChatModel _model = new UserChatModel();     //用户聊天模型
        
        #endregion

        #region 属性
        /// <summary>
        /// 聊天数据服务
        /// </summary>
        public DataService UserDataService 
        { 
            get => _userDataService; 
            private set => _userDataService = value; 
        }

        /// <summary>
        /// 用户聊天模型
        /// </summary>
        public UserChatModel Model
        { 
            get => _model;
            set
            {
                if (_model != value)
                {
                    _model = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region 命令
        /// <summary>
        /// 展开功能菜单命令
        /// </summary>
        public ICommand SelecteAddFileCommand { get; private set; }

        /// <summary>
        /// 提交命令
        /// </summary>
        public ICommand SubmitQuestionCommand { get; private set; }

        /// <summary>
        /// 鼠标滚动
        /// </summary>
        public ICommand MouseWheelCommand { get; private set; }

        /// <summary>
        /// 鼠标按下
        /// </summary>
        public ICommand MouseDownCommand { get; private set; }

        /// <summary>
        /// Markdown对象命令
        /// </summary>
        public ICommand MarkdownOBJCommand { get; private set; }
        
        /// <summary>
        /// 滑动条加载
        /// </summary>
        public ICommand ScrollLoadedCommand { get; private set; }
        /// <summary>
        /// 外部数据面板加载
        /// </summary>
        public ICommand ExternalDataPanelLoadedCommand { get; private set; }
        #endregion

        #endregion

        #region 构造函数
        public UserChatViewModel()
        {
            Initialize();
        }
        #endregion

        #region 初始化方法
        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Initialize()
        {
            //初始化命令
            SelecteAddFileCommand = new ParameterCommand(OnSelecteAddFile);
            MouseWheelCommand = new EventsCommand<MouseWheelEventArgs>(OnMouseWheel);
            MouseDownCommand = new EventsCommand<MouseButtonEventArgs>(OnMouseDown);
            MarkdownOBJCommand = new EventsCommand<object>(OnMarkdownOBJ);
            SubmitQuestionCommand = new ParameterlessCommand(OnSubmitQuestion);
            ScrollLoadedCommand = new EventsCommand<RoutedEventArgs>(OnScrollLoaded);
            ExternalDataPanelLoadedCommand = new EventsCommand<RoutedEventArgs>(OnExternalDataPanelLoaded);
            //按钮名称
            Model.SubmitButtonName = "提交";

            //聊天记录
            Model.Directory = $"{AppDomain.CurrentDomain.BaseDirectory}\\Record\\";
            UserDataService = new DataService($"{Model.Directory}");
            Model.FileName = UserDataService.FileModel.FileNameDT;
        }

        /// <summary>
        ///  水平排列容器加载
        /// </summary>
        private void OnExternalDataPanelLoaded(RoutedEventArgs args)
        {
            if (args.Source is WrapPanel wrapP)
            {
                wrapPanel = wrapP;
                Debug.Print("wrapPanel loaded...");
            }
        }
        #endregion

        #region 命令方法

        /// <summary>
        /// 加载文件:选择添加文件，最多添加10个文件
        /// </summary>
        private void OnSelecteAddFile(object obj)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "(*.txt;*,png;*.jpg;*.jpeg;*.bmp)|*.txt;*.png;*.jpg;*.jpeg;*.bmp|(*.png)|*.png|(*.*)|*.*";
            openFile.Multiselect = true;
            if (openFile.ShowDialog() == true)
            {
                string[] files = openFile.FileNames;
                //多个文件时创建外部数据
                if (files.Count() > 1)
                {
                    wrapPanel.Children.Clear();
                    foreach (var file in files)
                    {
                        Debug.Print(file);
                        if (UserDataService.ExternalDataCount < 10)
                        {
                            ExternalDataService dataObj = ExternalDataService.GeneratePreObject(UserDataService.DataID, file, BtnRemoveControl_Click);
                            UserDataService.AddExternaalData(dataObj);
                            wrapPanel.Children.Add(dataObj.View);
                        }
                    }
                }
                //单个文件时创建外部数据
                else
                {
                    Debug.Print(openFile.FileName);
                    if (UserDataService.ExternalDataCount < 10)
                    {
                        ExternalDataService dataObj = ExternalDataService.GeneratePreObject(UserDataService.DataID, openFile.FileName, BtnRemoveControl_Click);
                        UserDataService.AddExternaalData(dataObj);
                        wrapPanel.Children.Add(dataObj.View);
                    }
                }
            }
        }
        /// <summary>
        /// 提交:  提交问题到AI并获取返回结果
        /// </summary>
        private async void OnSubmitQuestion()
        {
            _ = Task.Delay(1);
            string input = Model.InputText;
            ChatDataModel chatData = new ChatDataModel();
            chatData.JsonModel.Role = "User";         
            chatData.JsonModel.Content = input;
            chatData.JsonModel.Date = DateTime.Now.ToString();
            string appendText = string.Empty;
            string tempText = string.Empty;
            try
            {
                if (!SubmintChecked(input))
                {
                    //直接回答完成：设置输入框为空，不自动滚动，按钮名称，按钮使能
                    Model.IsAutoScrolling = false;
                    OnStopCurrentChat();
                    Model.SubmitButtonName = "提交";
                    return;
                }
                Model.SubmitButtonName = "停止";
                Model.IsAutoScrolling = true;
                AppendText($"##{Environment.NewLine}");
                AppendText($"### [{chatData.JsonModel.Date}]{Environment.NewLine}");
                AppendText($"# 【User】{Environment.NewLine}");
                AppendText($"**{chatData.JsonModel.Content}**{Environment.NewLine}");
                AppendText($"---{Environment.NewLine}");
                AppendText($"{Environment.NewLine}");
                scrollViewer.ScrollToEnd();
                IEnumerable<string> imageBase64 = null;
                int index = 1;
                
                //加载文本|图像文件数据
                foreach (var data in UserDataService.ExternalDatas)
                {
                    //加载图像数据
                    if (data.Model.DataType == ExternalDataType.Image)
                    {
                        imageBase64 = ConvertImageToBase64IEnumerable(data.Model.FileName);
                        Message externalMessage = new Message();
                        externalMessage.Role = ChatRole.User;
                        externalMessage.Content = $"这是图像信息，如果用户有提到，可能是这些图像";
                        externalMessage.Images = imageBase64.ToArray();
                        Model.Ollama.Chat.Messages.Add(externalMessage);
                        string ImageUri = "![Local Image](file:///" + data.Model.FileName.Replace("\\", "/") + ")";
                        appendText += $"{ImageUri}{Environment.NewLine}{Environment.NewLine}";
                        AppendText($"{ImageUri}{Environment.NewLine}{Environment.NewLine}");
                    }
                    //加载文本数据
                    if (data.Model.DataType == ExternalDataType.Text)
                    {
                        Message externalFileMessage = new Message();
                        externalFileMessage.Content = $"文件名{Path.GetFileName(data.Model.FileName)},以下是这一份文件内容:如果用户有问到文件，可能是这些内容：\\n{File.ReadAllText(data.Model.FileName)}";
                        externalFileMessage.Role = ChatRole.User;
                        Model.Ollama.Chat.Messages.Add(externalFileMessage);
                    }
                }
                AppendText($"## 【AI】{Environment.NewLine}");

                //异步获取AI回答
                _cts_ChatThread = new CancellationTokenSource();
                _cts_MessageQueue = new CancellationTokenSource();
                _messageQueue = new ConcurrentQueue<string>();
                await foreach (var answerToken in Model.Ollama.Chat.SendAsync(input, imageBase64, _cts_ChatThread.Token))
                {
                    if (answerToken.Equals("\n") || answerToken.Equals("\r\n"))
                    {
                        Debug.Print(answerToken);
                    }
                    appendText += answerToken;
                    AppendText(answerToken);
                    await Task.Delay(20);
                    if (Model.IsAutoScrolling) scrollViewer.ScrollToEnd();//是否自动滚动
                }
                AppendText($"{Environment.NewLine}{Environment.NewLine}");
                chatData.JsonModel.Result = appendText;
                //创建聊天数据模型，保存记录
                chatData.Uri = UserDataService.FileModel.FileNameDT;
                DataService.AppendDataToJsonFile(chatData);

                AppendText($"---{Environment.NewLine}");
                //提交回答完后清空外部数据
                wrapPanel.Children.Clear(); 
                UserDataService.ClearExternalData();
            }
            catch (Exception ex)
            {
                AppendText($"Error: {ex.Message}");
                AppendText($"{Environment.NewLine}{Environment.NewLine}");
            }
            //回答完成：设置输入框为空，不自动滚动，按钮名称
            Model.InputText = string.Empty;
            Model.IsAutoScrolling = false;
            Model.SubmitButtonName = "提交";
        }

        /// <summary>
        /// 停止当前聊天
        /// </summary>
        private void OnStopCurrentChat()
        {
            _cts_ChatThread?.CancelAsync();
            _cts_MessageQueue?.CancelAsync();
            AppendText($"{Environment.NewLine}");
            Debug.Print("取消提问-----------------------");
            Thread.Sleep(10);
        }
        /// <summary>
        /// 鼠标滚动上下滑动
        /// </summary>
        private void OnMouseWheel(MouseWheelEventArgs e)
        {
            try
            {
                // 获取 ScrollViewer 对象
                if (e.Source is FrameworkElement element && element.Parent is ScrollViewer scrollViewer)
                {
                    // 获取当前的垂直偏移量
                    double currentOffset = scrollViewer.VerticalOffset;
                    if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToVerticalOffset(currentOffset - e.Delta);
                    }
                    else
                    {
                        scrollViewer.ScrollToVerticalOffset(currentOffset - e.Delta);
                    }
                    // 标记事件已处理，防止默认滚动行为
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        /// <summary>
        /// Markdown中鼠标按下
        /// </summary>
        private void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                Model.IsAutoScrolling = false;
                Debug.Print("Mouse Down...");
            }
        }

        /// <summary>
        /// 滚动栏触发
        /// </summary>
        private void OnScrollLoaded(RoutedEventArgs args)
        {
            if (args.Source is ScrollViewer scrollView)
            {
                scrollViewer = scrollView;
                Debug.Print("Scroll loaded...");
            }
        }

        /// <summary>
        /// Markdown控件对象更新触发
        /// </summary>
        private void OnMarkdownOBJ(object obj)
        {
            if (_markdownViewer != null) return;
            if (obj is MarkdownViewer markdownViewer)
            {
                _markdownViewer = markdownViewer;
                _markdownViewer.Markdown = string.Empty;
            }
        }
        #endregion

        #region 其他方法

        /// <summary>
        /// 输出文本
        /// </summary>
        public void AppendText(string newText)
        {
            Debug.Print(newText);
            Model.IsShowRunState = false;
            _markdownViewer.Markdown += newText;
        }

        /// <summary>
        /// 提交校验
        /// </summary>
        private bool SubmintChecked(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Trim().Length<2) return false;
            if (Model.SubmitButtonName.Equals("停止")) return false;
            return true;
        }

        /// <summary>
        /// 获取Markdown格式文本
        /// </summary>
        public void GetMarkdownFormat(string text)
        {
            // 解析Markdown
            var pipeline = new MarkdownPipelineBuilder().Build();
            var document = Markdig. Markdown.Parse(text, pipeline);
            // 遍历语法树并输出结果
            StringBuilder output = new StringBuilder();
            TraverseMarkdown(document, output);
            // 显示结果
            ChatJsonDataModel chatJsonDataModel = new ChatJsonDataModel();
            chatJsonDataModel.Result = output.ToString();
        }

        /// <summary>
        /// 遍历语法树：输出代码块字符串
        /// </summary>
        private void TraverseMarkdown(MarkdownObject markdownObject, StringBuilder output)
        {
            //代码块
            if (markdownObject is FencedCodeBlock codeBlock)
            {
                output.AppendLine(codeBlock.Lines.ToString());
            }
            else if (markdownObject is ContainerBlock containerBlock)
            {
                foreach (var child in containerBlock)
                {
                    TraverseMarkdown(child, output);
                }
            }
            else if (markdownObject is LeafBlock leafBlock)
            {
                output.AppendLine(leafBlock.Lines.ToString());
            }
        }

        /// <summary>
        /// 转换输出Markdown文本
        /// </summary>
        private void ConvertMarkdownOut(string text)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            // 应用样式
            _markdownViewer.Pipeline = pipeline;
            _markdownViewer.Markdown = text;
        }

        /// <summary>
        ///  移除控件：移除外部数据预览控件
        /// </summary>
        private void BtnRemoveControl_Click(object sender, RoutedEventArgs e)
        {
            // 获取触发事件的按钮
            Button removeButton = sender as Button;
            if (removeButton != null)
            {
                removeButton.Click -= BtnRemoveControl_Click;
                var dataView = UserDataService.ExternalDatas.FirstOrDefault(obj => obj.Model.Index.Equals(removeButton.Tag));
                wrapPanel.Children.Remove(dataView.View);
                UserDataService.ExternalDatas.Remove(dataView);
            }
        }

        public OllamaSharp.Models.Chat.Message GenerateMessage(string content, string role = "user")
        {
            return new OllamaSharp.Models.Chat.Message()
            {
                Content = content,
                Role = new ChatRole(role)
            };
        }
        #endregion

        #region 图像功能
        /// <summary>
        /// 转换图像为Base64
        /// </summary>
        private IEnumerable<IEnumerable<byte>> ConvertImagesToBytes(string imagePaths)
        {
            List<IEnumerable<byte>> imagesAsBytes = new List<IEnumerable<byte>>();
            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePaths);
                imagesAsBytes.Add(imageBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to convert image to bytes: " + ex.Message);
            }
            return imagesAsBytes;
        }

        /// <summary>
        /// 转换图像为Base64
        /// </summary>
        private string ConvertImageToBase64(string imagePath)
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to convert image to Base64: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 从字节中加载图像
        /// </summary>
        public void LoadImagesFromBytes(IEnumerable<IEnumerable<byte>> imagesAsBytes)
        {
            foreach (IEnumerable<byte> imageBytes in imagesAsBytes)
            {
                try
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(imageBytes.ToArray());
                    bitmapImage.EndInit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 转换图像到base64 IEnumerable
        /// </summary>
        private IEnumerable<string> ConvertImageToBase64IEnumerable(string imagePath)
        {
            string base64String = ConvertImageToBase64(imagePath);
            return new List<string> { base64String };
        }

        /// <summary>
        /// 从Base 64加载图像
        /// </summary>
        public void LoadImagesFromBase64(IEnumerable<string> imagesAsBase64)
        {
            foreach (string base64String in imagesAsBase64)
            {
                try
                {
                    string base64Data = base64String.Split(',').Last();
                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(imageBytes);
                    bitmapImage.EndInit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image: " + ex.Message);
                }
            }
        }
        #endregion

        #region 回调方法
        /// <summary>
        ///  加载聊天记录回调:
        ///     1、通过事件绑定触发该方法，传递文件路径，
        ///     2、通过聊天数据服务对象从文件中读取数据（Json序列化），返回聊天数据集合。
        /// </summary>
        public void LoadChatRecordCallback(string path)
        {
            Debug.Print(path);
            List<ChatJsonDataModel> datas = DataService.ReadDataFormJsonFile(path);
            StringBuilder appendText = new StringBuilder();
            //切换聊天记录时，清空Chat对象中的消息。
            Model.Ollama.Chat.Messages.Clear();        
            foreach (ChatJsonDataModel data in datas)
            {
                //文本追加、添加到Markdown控件中
                appendText.AppendLine($"### {data.Date}{Environment.NewLine}");
                appendText.AppendLine($"## 【{data.Role}】{Environment.NewLine}");
                appendText.AppendLine($"#### {data.Content}{Environment.NewLine}");
                AppendText($"---{Environment.NewLine}");
                appendText.AppendLine($"## 【AI】{Environment.NewLine}");
                appendText.AppendLine($"{data.Result}{Environment.NewLine}");
                appendText.AppendLine($"{Environment.NewLine}");
                //创建并添加消息到Chat中
                Model.Ollama.Chat.Messages.Add(GenerateMessage(data.Content));
                Model.Ollama.Chat.Messages.Add(GenerateMessage(data.Result, "assistant"));
            }
            //设置当前路径为该文件路径，方便提问时将内容续写到该文件中
            UserDataService.FileModel.FileNameDT = path;  
            //显示内容
            ConvertMarkdownOut(appendText.ToString());
            scrollViewer.ScrollToTop();    //滚动到顶部
        }
        
        #endregion
    }
}
