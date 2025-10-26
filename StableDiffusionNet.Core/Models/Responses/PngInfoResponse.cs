namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ с метаданными PNG изображения
    /// </summary>
    public class PngInfoResponse
    {
        /// <summary>
        /// Извлеченная информация о генерации (промпт, параметры и т.д.)
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// Извлеченные элементы (items)
        /// </summary>
        public object? Items { get; set; }
    }
}
