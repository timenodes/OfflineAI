using System.IO;
using Newtonsoft.Json;

namespace OfflineAI.Services
{
    /// <summary>
    /// 序列化服务
    ///     1、2025-3-01 序列化数据到Json文件
    ///     2、2025-3-01 反序列化Json文件, 转换为指定类型的对象
    ///     3、2025-3-01 反序列化JSON 字符串，转换为指定类型的对象
    /// </summary>
    public class SerializeService
    {
        /// <summary>
        /// 序列化：将对象转换为 JSON 字符串
        /// </summary> 
        public static string SerializeToJson<T>(T data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"序列化失败: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 反序列Json文件，转换为指定类型的对象
        /// </summary>
        public static T DeSerializeFormFile<T>(string fileName)
        {
            try
            {
                string json = File.ReadAllText(fileName);
                T data = JsonConvert.DeserializeObject<T>(json);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"反序列化失败: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 反序列化JSON 字符串，转换为指定类型的对象
        /// </summary>
        public static T DeserializeFromJson<T>(string json)
        {
            try
            {
                T data = JsonConvert.DeserializeObject<T>(json);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"反序列化失败: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 反序列化Json字符串到集合
        /// </summary>
        public static List<T> DeserializeJsonToList<T>(string json)
        {
            try
            {
                List<T> dataList = DeserializeFromJson<List<T>>(json);
                return dataList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"反序列化失败: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 反序列化Json文件到集合
        /// </summary>
        public static List<T> DeserializeJsonFileToList<T>(string fileName)
        {
            try
            {
                List<T> dataList = DeSerializeFormFile<List<T>>(fileName);
                return dataList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"反序列化失败: {ex.Message}");
                return default;
            }
        }
    }
}
