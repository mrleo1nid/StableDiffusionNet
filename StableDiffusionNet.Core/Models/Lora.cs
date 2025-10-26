namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация о LoRA модели
    /// </summary>
    public class Lora
    {
        /// <summary>
        /// Имя LoRA модели
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Алиас (псевдоним)
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Путь к файлу модели
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Метаданные модели
        /// </summary>
        public object? Metadata { get; set; }
    }
}
