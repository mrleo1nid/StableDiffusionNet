using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация об апскейлере
    /// </summary>
    public class Upscaler
    {
        /// <summary>
        /// Название апскейлера
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Имя модели (если применимо)
        /// </summary>
        [JsonProperty("model_name")]
        public string? ModelName { get; set; }

        /// <summary>
        /// Путь к модели (если применимо)
        /// </summary>
        [JsonProperty("model_path")]
        public string? ModelPath { get; set; }

        /// <summary>
        /// URL модели (если применимо)
        /// </summary>
        [JsonProperty("model_url")]
        public string? ModelUrl { get; set; }

        /// <summary>
        /// Масштаб апскейлера
        /// </summary>
        [JsonProperty("scale")]
        public double? Scale { get; set; }
    }
}
