using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ постобработки изображения
    /// </summary>
    public class ExtraSingleImageResponse
    {
        /// <summary>
        /// Base64-закодированное обработанное изображение
        /// </summary>
        [JsonProperty("image")]
        public string? Image { get; set; }

        /// <summary>
        /// HTML с дополнительными результатами (если запрошено)
        /// </summary>
        [JsonProperty("html_info")]
        public string HtmlInfo { get; set; } = string.Empty;
    }
}
