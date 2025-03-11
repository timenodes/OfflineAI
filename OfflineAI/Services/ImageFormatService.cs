using System.Drawing.Imaging;
using System.IO;

namespace OfflineAI.Services
{
    /// <summary>
    /// 图像格式工具：获取正确的图像格式，通过图像文件的二进制头部图像格式标识。
    /// </summary>
    public class ImageFormatService
    {
        #region 判断图像的正确格式
        /// <summary>
        /// 图像格式工具：获取正确的图像格式，通过图像文件的二进制头部图像格式标识。
        /// </summary>
        public static ImageFormat GetImageFormat(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    // 读取文件的前几个字节
                    byte[] headerBytes = br.ReadBytes(16);

                    // 根据文件的前几个字节判断图像的实际格式
                    if (IsJpeg(headerBytes))
                    {
                        return ImageFormat.Jpeg;
                    }
                    else if (IsPng(headerBytes))
                    {
                        return ImageFormat.Png;
                    }
                    else if (IsGif(headerBytes))
                    {
                        return ImageFormat.Gif;
                    }
                    else if (IsBmp(headerBytes))
                    {
                        return ImageFormat.Bmp;
                    }
                    else
                    {
                        // 默认返回未知格式
                        return null;
                    }
                }
            }
        }
        /// <summary>
        /// JPEG 文件的前两个字节是 0xFF, 0xD8
        /// </summary>
        private static bool IsJpeg(byte[] headerBytes)
        {
            
            return headerBytes.Length >= 2 && headerBytes[0] == 0xFF && headerBytes[1] == 0xD8;
        }
        /// <summary>
        /// PNG 文件的前八个字节是固定的签名：137 80 78 71 13 10 26 10
        /// </summary>
        private static bool IsPng(byte[] headerBytes)
        {
            return headerBytes.Length >= 8 && headerBytes[0] == 137
                    && headerBytes[1] == 80 && headerBytes[2] == 78
                    && headerBytes[3] == 71 && headerBytes[4] == 13
                    && headerBytes[5] == 10 && headerBytes[6] == 26
                    && headerBytes[7] == 10;
        }
        /// <summary>
        /// GIF 文件的前三个字节是 "GIF"
        /// </summary>
        private static bool IsGif(byte[] headerBytes)
        {
            return headerBytes.Length >= 3 && headerBytes[0] == 71
                    && headerBytes[1] == 73 && headerBytes[2] == 70;
        }
        /// <summary>
        /// BMP 文件的前两个字节是 "BM"
        /// </summary>
        private static bool IsBmp(byte[] headerBytes)
        {
            return headerBytes.Length >= 2 && headerBytes[0] == 66
                && headerBytes[1] == 77;
        }
        #endregion

    }
}
