using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о режиме latent upscale
    /// </summary>
    public class LatentUpscaleMode
    {
        /// <summary>
        /// Название режима
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
