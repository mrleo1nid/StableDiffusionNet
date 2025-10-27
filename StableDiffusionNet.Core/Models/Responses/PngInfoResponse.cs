namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ с метаданными PNG изображения.
    /// Immutable record для безопасности и удобства работы с данными.
    /// </summary>
    public record PngInfoResponse
    {
        /// <summary>
        /// Извлеченная информация о генерации (промпт, параметры и т.д.)
        /// </summary>
        public string Info { get; init; } = string.Empty;

        /// <summary>
        /// Извлеченные элементы (items)
        /// </summary>
        public object? Items { get; init; }
    }
}
