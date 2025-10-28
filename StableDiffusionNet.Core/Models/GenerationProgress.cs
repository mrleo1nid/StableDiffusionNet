using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Прогресс генерации изображения
    /// </summary>
    public class GenerationProgress
    {
        /// <summary>
        /// Прогресс от 0 до 1
        /// </summary>
        [JsonProperty("progress")]
        public double Progress { get; set; }

        /// <summary>
        /// Примерное время до завершения в секундах
        /// </summary>
        [JsonProperty("eta_relative")]
        public double EtaRelative { get; set; }

        /// <summary>
        /// Состояние генерации
        /// </summary>
        [JsonProperty("state")]
        public ProgressState? State { get; set; }

        /// <summary>
        /// Текущее изображение (preview) в base64
        /// </summary>
        [JsonProperty("current_image")]
        public string? CurrentImage { get; set; }

        /// <summary>
        /// Текстовая информация о состоянии
        /// </summary>
        [JsonProperty("textinfo")]
        public string? TextInfo { get; set; }
    }
}
