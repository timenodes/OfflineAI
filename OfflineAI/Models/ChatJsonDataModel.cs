using Newtonsoft.Json;

namespace OfflineAI.Models
{
    /// <summary>
    /// 聊天（Json）数据
    /// </summary>
    public class ChatJsonDataModel: PropertyChangedBase
    {
        /// <summary>
        /// 角色
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// 图像
        /// </summary>
        [JsonProperty("images[]")]
        public string Image { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        [JsonProperty("result")]
        public string Result { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; }
    }
}
