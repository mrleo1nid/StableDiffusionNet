using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Прогресс генерации изображения
    /// </summary>
    public class GenerationProgress
    {
        /// <summary>
        /// Прогресс от 0 до 1
        /// </summary>
        [JsonProperty("progress")]
        public double Progress { get; set; }

        /// <summary>
        /// Примерное время до завершения в секундах
        /// </summary>
        [JsonProperty("eta_relative")]
        public double EtaRelative { get; set; }

        /// <summary>
        /// Состояние генерации
        /// </summary>
        [JsonProperty("state")]
        public ProgressState? State { get; set; }

        /// <summary>
        /// Текущее изображение (preview) в base64
        /// </summary>
        [JsonProperty("current_image")]
        public string? CurrentImage { get; set; }

        /// <summary>
        /// Текстовая информация о состоянии
        /// </summary>
        [JsonProperty("textinfo")]
        public string? TextInfo { get; set; }
    }

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
