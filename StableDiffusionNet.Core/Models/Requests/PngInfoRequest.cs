using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Requests
{
    /// <summary>
    /// Запрос для извлечения метаданных из PNG изображения
    /// </summary>
    public class PngInfoRequest
    {
        /// <summary>
        /// Base64-закодированное PNG изображение
        /// </summary>
        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;
    }
}
