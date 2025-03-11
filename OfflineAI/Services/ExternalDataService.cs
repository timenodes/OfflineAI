using OfflineAI.Models;
using OfflineAI.ViewModels;
using OfflineAI.Views.UserViews;
using System.IO;
using System.Windows;
namespace OfflineAI.Services
{
    /// <summary>
    /// 外部数据对象：
    /// </summary>
    public class ExternalDataService
    {
        public ExternalDataService()
        {
        }
        public ExternalDataService(ExternalDataPreView view)
        {
            View = view;
        }
        /// <summary>
        /// 外部数据预览视图模型
        /// </summary>
        public ExternalDataPreViewModel? ViewModel { get => View?.ViewModel; }
        /// <summary>
        /// 外部数据预览视图
        /// </summary>
        public ExternalDataPreView? View { get; set; }
        /// <summary>
        /// 外部数据预览模型
        /// </summary>
        public ExternalDataPreModel? Model { get => ViewModel?.Model; }

        /// <summary>
        /// 生成外部数据预览对象：
        ///     1、索引、
        ///     2、文件路径、
        ///     3、点击事件回调
        /// </summary>
        public static ExternalDataService GeneratePreObject(object index, string filePath, RoutedEventHandler clickCallback)
        {
            //创建外部数据预览面板
            FileInfo fileInfo = new FileInfo(filePath);
            ExternalDataPreView preView = new ExternalDataPreView();
            preView.ViewModel.Model.Index = index;
            preView.ViewModel.Model.FileName = filePath;
            preView.ViewModel.Model.DataType = DataService.GetFileType(filePath);
            preView.RemoveButton.Tag = preView.ViewModel.Model.Index;
            preView.Height = 32;
            preView.ViewModel.SetImageSource(DataService.GetFileType(filePath));
            preView.Tbx_Text.Clear();
            preView.Tbx_Text.AppendText($"{Path.GetFileName(filePath)}{Environment.NewLine}");
            preView.Tbx_Text.AppendText($"大小{FormatFileSize(fileInfo.Length)}");
            preView.RemoveButton.Click += clickCallback;
            return new ExternalDataService(preView);
        }
        /// <summary>
        /// 文件大小格式
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }
    }
}
