using Newtonsoft.Json;

namespace OfflineAI.Models
{
    public class ConfigParameterModel
    {
        [JsonProperty("ModelName")]
        public string ModelName { get; set; }

        [JsonProperty("DataPath")]
        public string DataPath { get; set; }

        [JsonProperty("LastDataPath")]
        public string LastDataPath { get; set; }

    }
}
