using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ постобработки изображения.
    /// Immutable record для безопасности и удобства работы с данными.
    /// </summary>
    public record ExtraSingleImageResponse
    {
        /// <summary>
        /// Base64-закодированное обработанное изображение
        /// </summary>
        [JsonProperty("image")]
        public string? Image { get; init; }

        /// <summary>
        /// HTML с дополнительными результатами (если запрошено)
        /// </summary>
        [JsonProperty("html_info")]
        public string HtmlInfo { get; init; } = string.Empty;
    }
}
