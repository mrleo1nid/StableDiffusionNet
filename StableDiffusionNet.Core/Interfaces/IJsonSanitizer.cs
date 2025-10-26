namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для санитизации JSON для логирования
    /// </summary>
    internal interface IJsonSanitizer
    {
        /// <summary>
        /// Очищает JSON для безопасного логирования
        /// </summary>
        /// <param name="json">JSON строка для санитизации</param>
        /// <returns>Санитизированная JSON строка</returns>
        string SanitizeForLogging(string json);
    }
}
