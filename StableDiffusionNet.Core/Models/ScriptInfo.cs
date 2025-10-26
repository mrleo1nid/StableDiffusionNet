using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о скрипте
    /// </summary>
    public class ScriptInfo
    {
        /// <summary>
        /// Название скрипта
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Является ли скрипт alwayson
        /// </summary>
        [JsonProperty("is_alwayson")]
        public bool? IsAlwayson { get; set; }

        /// <summary>
        /// Является ли скрипт img2img
        /// </summary>
        [JsonProperty("is_img2img")]
        public bool? IsImg2img { get; set; }

        /// <summary>
        /// Список аргументов скрипта
        /// </summary>
        [JsonProperty("args")]
        public List<ScriptArg> Args { get; set; } = new List<ScriptArg>();
    }

    /// <summary>
    /// Аргумент скрипта
    /// </summary>
    public class ScriptArg
    {
        /// <summary>
        /// Название аргумента в UI
        /// </summary>
        [JsonProperty("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        [JsonProperty("value")]
        public object? Value { get; set; }

        /// <summary>
        /// Минимальное значение
        /// </summary>
        [JsonProperty("minimum")]
        public object? Minimum { get; set; }

        /// <summary>
        /// Максимальное значение
        /// </summary>
        [JsonProperty("maximum")]
        public object? Maximum { get; set; }

        /// <summary>
        /// Шаг изменения значения
        /// </summary>
        [JsonProperty("step")]
        public object? Step { get; set; }

        /// <summary>
        /// Возможные значения
        /// </summary>
        [JsonProperty("choices")]
        public List<string>? Choices { get; set; }
    }

    /// <summary>
    /// Список доступных скриптов
    /// </summary>
    public class ScriptsList
    {
        /// <summary>
        /// Скрипты для txt2img
        /// </summary>
        [JsonProperty("txt2img")]
        public List<string>? Txt2img { get; set; }

        /// <summary>
        /// Скрипты для img2img
        /// </summary>
        [JsonProperty("img2img")]
        public List<string>? Img2img { get; set; }
    }
}
