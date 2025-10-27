using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ на запрос генерации изображения из текста.
    /// Immutable record для безопасности и удобства работы с данными.
    /// </summary>
    public record TextToImageResponse
    {
        /// <summary>
        /// Сгенерированные изображения в base64
        /// </summary>
        [JsonProperty("images")]
        public IReadOnlyList<string>? Images { get; init; }

        /// <summary>
        /// Параметры генерации
        /// </summary>
        [JsonProperty("parameters")]
        public IReadOnlyDictionary<string, object>? Parameters { get; init; }

        /// <summary>
        /// Информация о генерации
        /// </summary>
        [JsonProperty("info")]
        public string? Info { get; init; }
    }
}
