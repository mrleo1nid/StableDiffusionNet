using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ постобработки батча изображений
    /// </summary>
    public class ExtrasBatchImagesResponse
    {
        /// <summary>
        /// HTML информация о процессе обработки
        /// </summary>
        [JsonProperty("html_info")]
        public string HtmlInfo { get; set; } = string.Empty;

        /// <summary>
        /// Список обработанных изображений в base64
        /// </summary>
        [JsonProperty("images")]
        public List<string> Images { get; set; } = new List<string>();
    }
}
