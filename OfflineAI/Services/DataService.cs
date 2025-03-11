using OfflineAI.ExtensionTool;
using OfflineAI.Models;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;

namespace OfflineAI.Services
{
    /// <summary>
    /// 描述：数据服务类：用于处理聊天数据相关的操作。
    /// 作者：吾与谁归
    /// 时间: 2025年2月25日
    /// 功能：
    /// 版本version: 1.1
    ///     1、 2025-02-25 添加获取指定目录下的所有文件（*.json）功能
    ///     2、 2025-03-02 添加创建日期目录、创建日期文件名功能。
    ///     3、 2025-03-02 添加写入数据到Json文件。
    ///     4、 2025-03-02 添加读取Json文件中的数据集
    ///     5、 2025-03-03 添加外部数据加载功能。
    ///     6、 2025-03-05 新增字段、属性：外部数据ID、个数、集合。
    ///     7、 2025-03-05 添加外部数据加载方法。
    ///     8、 2025-03-08 添加检测文件格式、输出文件类型（Text、Image、Unknown）。
    /// </summary>
    public class DataService
    {
        #region 字段、属性
        /// <summary>
        /// 拓展名列表
        /// </summary>
        public static List<string> Extensions = new List<string>
        {
            // 文本文件
            ".txt", ".csv", ".json", ".xml", ".html", ".htm", ".md",
            // 代码文件
            ".py", ".java", ".cs", ".cpp", ".c", ".js", ".php",
            // 文档文件
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".rtf",
            // 图像文件
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff",
            // 音频文件
            ".mp3", ".wav", ".ogg", ".flac",
            // 视频文件
            ".mp4", ".avi", ".mkv", ".mov",
            // 压缩文件
            ".zip", ".rar", ".tar", ".gz",
            // 数据库文件
            ".sql", ".db", ".mdb", ".accdb",
            // 配置文件
            ".ini", ".yaml", ".yml", ".toml",
            // 其他文件
            ".log", ".dat", ".bin"
        };
        /// <summary>
        /// 外部数据模型集合:当前最大10个
        /// </summary>
        private LimitedObservableCollection<ExternalDataService> _externalDatas 
            = new LimitedObservableCollection<ExternalDataService>(10);
        /// <summary>
        /// 数据索引
        /// </summary>
        public int DataID { get; private set; }
        /// <summary>
        /// 外部数据模型集合个数
        /// </summary>
        public int ExternalDataCount => ExternalDatas.Count;
        /// <summary>
        /// 外部数据模型集合
        /// </summary>
        public LimitedObservableCollection<ExternalDataService> ExternalDatas { get => _externalDatas; }
        /// <summary>
        /// 文件操作模型
        /// </summary>
        private FileOperationModel _fileModel = new FileOperationModel();
        /// <summary>
        /// 文件操作模型
        /// </summary>
        public FileOperationModel FileModel
        {
            get => _fileModel;
            private set => _fileModel = value;
        }
        #endregion

        #region 构造函数

        public DataService()
        {
            FileModel = new FileOperationModel();
        }
        public DataService(string directory)
        {
            FileModel = new FileOperationModel()
            {
                IsGenerateDirectory = true,         //是否生成目录
                Directory = directory,              //目录
                FileNameFormat = "yyyyMMddHHmmss",  //生成文件名格式
                Extension = "json",                 //拓展名
            };
            FileModel.FileName = $"{directory}{GenFileNameDT(FileModel.FileNameFormat,FileModel.Extension)}";
            GenerateDirectoryDT();
            GenerateFileNameDT(FileModel.FileNameFormat, FileModel.Extension);
        }
        ~DataService()
        {
        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 生成日期目录路径
        /// </summary>
        public void GenerateDirectoryDT()
        {
            //创建日期目录1
            string filePath = $"{FileModel.Directory}\\{DateTime.Now.ToString("yyyy")}";
            Directory.CreateDirectory($"{filePath}");
            //创建日期目录2
            filePath = $"{filePath}\\{DateTime.Now.ToString("yyyyMMdd")}\\";
            Directory.CreateDirectory($"{filePath}");
            //设置日期目录
            FileModel.DirectoryDT = filePath;
        }

        /// <summary>
        /// 生成日期文件名
        /// </summary>
        public void GenerateFileNameDT(string timeFormat, string extension)
        {
            FileModel.FileNameDT = $"{FileModel.DirectoryDT}" +
                $"{DateTime.Now.ToString(timeFormat)}.{extension}";
        }

        /// <summary>
        /// 更新路径
        /// </summary>
        public void UpdatePath(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);
            string directory = Path.GetDirectoryName(filePath);
            FileModel.Extension = extension;
            FileModel.FileName = fileName;
            FileModel.Directory = directory;
            GenerateDirectoryDT();
            GenerateFileNameDT(FileModel.FileNameFormat, FileModel.Extension);
        }

        /// <summary>
        /// 生成文件名，根据时间格式和扩展名生成文件名
        /// </summary>
        public string GenFileNameDT(string timeFormat, string extension)
        {
            return $"{DateTime.Now.ToString(FileModel.FileNameFormat)}.{FileModel.Extension}";
        }

        /// <summary>
        /// 添加外部数据
        /// </summary>
        public void AddExternaalData(ExternalDataService dataModel)
        {
            ExternalDatas.Add(dataModel);
            DataID++;
        }

        /// <summary>
        /// 清空外部数据
        /// </summary>
        public void ClearExternalData()
        {
            ExternalDatas.Clear();
            DataID = 0;
        }

        #region 静态方法
        /// <summary>
        /// 获取指定目录下的所有文件（*.json）
        /// </summary>
        public static string[] GetFiles(string directory)
        {
            string[] files = Directory.GetFiles(directory, $"*.Json", SearchOption.AllDirectories);
            return files;
        }

        /// <summary>
        /// 写入数据到文件：以追加的方式将数据写入到 JSON 文件，如果文件不存在，则创建一个新文件添加数据。
        /// </summary>
        public static void AppendDataToJsonFile(ChatDataModel dataModel)
        {
            List<ChatJsonDataModel>? datas;

            /// 如果文件存在，读取现有数据
            if (File.Exists(dataModel.Uri))
            {
                var json = File.ReadAllText(dataModel.Uri);
                datas = JsonSerializer.Deserialize<List<ChatJsonDataModel>>(json);
            }
            /// 如果文件不存在，创建一个空的列表
            else
            {
                datas = new List<ChatJsonDataModel>();
            }
            // 添加新数据
            datas?.Add(dataModel.JsonModel);

            // 将更新后的数据写回文件
            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(datas, options);
            File.WriteAllText(dataModel.Uri, updatedJson);
        }

        /// <summary>
        /// 读取Json文件中的数据
        /// </summary>
        public static List<ChatJsonDataModel>? ReadDataFormJsonFile(string fileName)
        {
            List<ChatJsonDataModel>? datas = null;
            if (File.Exists(fileName))
            {
                var json = File.ReadAllText(fileName);
                datas = JsonSerializer.Deserialize<List<ChatJsonDataModel>>(json);
                return datas;
            }
            return null;
        }

        /// <summary>
        /// 检查文件扩展名
        /// </summary>
        public static bool FileFilter(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (IsFileOfType(extension, ".txt")
             || IsFileOfType(extension, ".json")
             || IsFileOfType(extension, ".cs")
             || IsFileOfType(extension, ".xaml")
             || IsFileOfType(extension, ".js")
             || IsFileOfType(extension, ".css")
             || IsFileOfType(extension, ".cpp")
             || IsFileOfType(extension, ".c")
             || IsFileOfType(extension, ".py")
             || IsFileOfType(extension, ".xml")
             || IsFileOfType(extension, ".html")
             || IsFileOfType(extension, ".xml"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查文件扩展名是否与指定的扩展名匹配（不区分大小写）
        /// </summary>
        public static bool IsFileOfType(string extension, string type)
        {
            return extension.Equals(type, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 获取文件是否为图像
        /// </summary>
        public static ImageFormat GetFileIsImage(string filePath)
        {
            return ImageFormatService.GetImageFormat(filePath);
        }
        /// <summary>
        /// 获取文件是否为文本类型
        /// </summary>
        public static bool GetFileIsText(string filePath)
        {
            return FileFilter(filePath);
        }
        /// <summary>
        /// 获取文件类型
        /// </summary>
        public static ExternalDataType GetFileType(string filePath)
        {
            if (GetFileIsText(filePath))
            {
                return ExternalDataType.Text;
            }
            else if (GetFileIsImage(filePath) != null)
            {
                return ExternalDataType.Image;
            }
            else
            {
                return ExternalDataType.Unknown;
            }
        }
        #endregion

        #endregion
    }
}
