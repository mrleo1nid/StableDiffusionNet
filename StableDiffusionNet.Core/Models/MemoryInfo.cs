using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о памяти системы
    /// </summary>
    public class MemoryInfo
    {
        /// <summary>
        /// Информация о системной памяти (RAM)
        /// </summary>
        [JsonProperty("ram")]
        public Dictionary<string, object> Ram { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Информация о памяти CUDA (GPU)
        /// </summary>
        [JsonProperty("cuda")]
        public Dictionary<string, object> Cuda { get; set; } = new Dictionary<string, object>();
    }
}
