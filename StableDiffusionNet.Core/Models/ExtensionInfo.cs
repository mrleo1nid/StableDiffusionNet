using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о расширении WebUI
    /// </summary>
    public class ExtensionInfo
    {
        /// <summary>
        /// Название расширения
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL репозитория
        /// </summary>
        [JsonProperty("remote")]
        public string Remote { get; set; } = string.Empty;

        /// <summary>
        /// Ветка репозитория
        /// </summary>
        [JsonProperty("branch")]
        public string Branch { get; set; } = string.Empty;

        /// <summary>
        /// Hash коммита
        /// </summary>
        [JsonProperty("commit_hash")]
        public string CommitHash { get; set; } = string.Empty;

        /// <summary>
        /// Версия расширения
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Дата коммита (Unix timestamp)
        /// </summary>
        [JsonProperty("commit_date")]
        public long CommitDate { get; set; }

        /// <summary>
        /// Включено ли расширение
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}
