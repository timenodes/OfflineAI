namespace OfflineAI.Models
{
    /// <summary>
    /// 文件操作
    ///     1、是否生成目录
    ///     2、目录
    ///     3、日期目录
    ///     4、文件名
    ///     5、日期文件名
    ///     6、扩展名
    ///     7、文件名格式
    /// </summary>
    public class FileOperationModel : PropertyChangedBase
    {
        /// <summary>
        /// 是否生成目录
        /// </summary>
        public bool IsGenerateDirectory {  get; set; }

        /// <summary>
        /// 文件目录
        /// </summary>
        public string Directory {  get; set; }

        /// <summary>
        /// 日期目录（生成的目录）
        /// </summary>
        public string DirectoryDT { get; set; }

        /// <summary>
        /// 文件名称（全路径）
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件名称（生成文件全路径）
        /// </summary>
        public string FileNameDT { get; set; }

        /// <summary>
        /// 文件拓展名
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// 文件名格式
        /// </summary>
        public string FileNameFormat { get; set; }
    }
}
