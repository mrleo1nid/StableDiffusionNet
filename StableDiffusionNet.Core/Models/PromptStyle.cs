using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о сохраненном стиле промпта
    /// </summary>
    public class PromptStyle
    {
        /// <summary>
        /// Название стиля
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Промпт стиля
        /// </summary>
        [JsonProperty("prompt")]
        public string? Prompt { get; set; }

        /// <summary>
        /// Негативный промпт стиля
        /// </summary>
        [JsonProperty("negative_prompt")]
        public string? NegativePrompt { get; set; }
    }
}
