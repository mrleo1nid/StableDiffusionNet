using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Состояние процесса генерации
    /// </summary>
    public class ProgressState
    {
        /// <summary>
        /// Пропущено ли изображение
        /// </summary>
        [JsonProperty("skipped")]
        public bool Skipped { get; set; }

        /// <summary>
        /// Прервана ли генерация
        /// </summary>
        [JsonProperty("interrupted")]
        public bool Interrupted { get; set; }

        /// <summary>
        /// Название задачи
        /// </summary>
        [JsonProperty("job")]
        public string? Job { get; set; }

        /// <summary>
        /// Количество задач
        /// </summary>
        [JsonProperty("job_count")]
        public int JobCount { get; set; }

        /// <summary>
        /// Номер текущей задачи
        /// </summary>
        [JsonProperty("job_no")]
        public int JobNo { get; set; }

        /// <summary>
        /// Текущий шаг сэмплирования
        /// </summary>
        [JsonProperty("sampling_step")]
        public int SamplingStep { get; set; }

        /// <summary>
        /// Общее количество шагов сэмплирования
        /// </summary>
        [JsonProperty("sampling_steps")]
        public int SamplingSteps { get; set; }
    }
}
