using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ на запрос генерации изображения из изображения
    /// </summary>
    public class ImageToImageResponse
    {
        /// <summary>
        /// Сгенерированные изображения в base64
        /// </summary>
        [JsonProperty("images")]
        public List<string> Images { get; set; } = new List<string>();

        /// <summary>
        /// Параметры генерации
        /// </summary>
        [JsonProperty("parameters")]
        public Dictionary<string, object>? Parameters { get; set; }

        /// <summary>
        /// Информация о генерации
        /// </summary>
        [JsonProperty("info")]
        public string? Info { get; set; }
    }
}
