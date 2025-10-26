using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о модели восстановления лиц
    /// </summary>
    public class FaceRestorer
    {
        /// <summary>
        /// Название модели восстановления лиц
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Путь к директории с моделью
        /// </summary>
        [JsonProperty("cmd_dir")]
        public string? CmdDir { get; set; }
    }
}
