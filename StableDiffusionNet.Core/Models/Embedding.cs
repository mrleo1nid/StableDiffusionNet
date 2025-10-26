namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация об embedding (textual inversion)
    /// </summary>
    public class Embedding
    {
        /// <summary>
        /// Имя embedding
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Количество векторов
        /// </summary>
        public int? VectorCount { get; set; }

        /// <summary>
        /// Шаг обучения
        /// </summary>
        public int? Step { get; set; }

        /// <summary>
        /// Checkpoint модели, на которой обучался
        /// </summary>
        public string? SdCheckpoint { get; set; }

        /// <summary>
        /// Имя checkpoint (короткое)
        /// </summary>
        public string? SdCheckpointName { get; set; }
    }
}
