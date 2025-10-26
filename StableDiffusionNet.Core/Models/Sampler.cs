using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о sampler (методе семплирования)
    /// </summary>
    public class Sampler
    {
        /// <summary>
        /// Название sampler'а
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Альтернативные имена (алиасы)
        /// </summary>
        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; } = new List<string>();

        /// <summary>
        /// Дополнительные опции sampler'а
        /// </summary>
        [JsonProperty("options")]
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }
}
