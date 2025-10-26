using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о scheduler (планировщике шагов)
    /// </summary>
    public class Scheduler
    {
        /// <summary>
        /// Внутреннее имя scheduler'а
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Отображаемое имя scheduler'а
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Альтернативные имена (алиасы)
        /// </summary>
        [JsonProperty("aliases")]
        public List<string>? Aliases { get; set; }

        /// <summary>
        /// Значение rho по умолчанию
        /// </summary>
        [JsonProperty("default_rho")]
        public double? DefaultRho { get; set; }

        /// <summary>
        /// Требуется ли внутренняя модель
        /// </summary>
        [JsonProperty("need_inner_model")]
        public bool? NeedInnerModel { get; set; }
    }
}
