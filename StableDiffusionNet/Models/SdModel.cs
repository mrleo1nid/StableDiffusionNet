using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о модели Stable Diffusion
    /// </summary>
    public class SdModel
    {
        /// <summary>
        /// Название модели
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Имя файла модели
        /// </summary>
        [JsonProperty("model_name")]
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Hash модели
        /// </summary>
        [JsonProperty("hash")]
        public string? Hash { get; set; }

        /// <summary>
        /// SHA256 hash
        /// </summary>
        [JsonProperty("sha256")]
        public string? Sha256 { get; set; }

        /// <summary>
        /// Имя файла конфигурации
        /// </summary>
        [JsonProperty("config")]
        public string? Config { get; set; }
    }
}
