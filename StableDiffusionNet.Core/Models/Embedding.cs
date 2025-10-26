using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация об embedding (textual inversion)
    /// </summary>
    public class Embedding
    {
        /// <summary>
        /// Имя embedding
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Шаг обучения
        /// </summary>
        [JsonProperty("step")]
        public int? Step { get; set; }

        /// <summary>
        /// Hash checkpoint модели, на которой обучался
        /// </summary>
        [JsonProperty("sd_checkpoint")]
        public string? SdCheckpoint { get; set; }

        /// <summary>
        /// Имя checkpoint модели (короткое)
        /// </summary>
        [JsonProperty("sd_checkpoint_name")]
        public string? SdCheckpointName { get; set; }

        /// <summary>
        /// Размер каждого вектора в embedding
        /// </summary>
        [JsonProperty("shape")]
        public int Shape { get; set; }

        /// <summary>
        /// Количество векторов в embedding
        /// </summary>
        [JsonProperty("vectors")]
        public int Vectors { get; set; }
    }
}
