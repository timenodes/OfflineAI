using System.ComponentModel;

namespace OfflineAI.Models
{
    /// <summary>
    /// 文件变更
    /// </summary>
    public class FileChangedModel
    {
        [Description("文件名")]
        public string? FileName {  get; set; }

        [Description("操作")]
        public FileChangeType Options { get; set; }
    }

    /// <summary>
    /// 文件变更类型，创建，删除，修改
    /// </summary>
    public enum FileChangeType
    {
        [Description("文件被创建")]
        Created,

        [Description("文件被删除")]
        Deleted,

        [Description("文件被修改")]
        Modified
    }
}
