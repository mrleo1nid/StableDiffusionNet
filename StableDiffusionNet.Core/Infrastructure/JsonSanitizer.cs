namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Санитизатор JSON для безопасного логирования
    /// Single Responsibility - отвечает только за санитизацию JSON
    /// </summary>
    internal static class JsonSanitizer
    {
        /// <summary>
        /// Максимальная длина JSON для логирования без обрезки.
        /// 500 символов достаточно для отладки без засорения логов.
        /// Более длинные JSON обычно содержат base64 данные изображений,
        /// которые не имеют смысла в логах и занимают много места.
        /// </summary>
        private const int MaxLength = 500;

        /// <summary>
        /// Очищает JSON для безопасного логирования
        /// </summary>
        /// <param name="json">JSON строка для санитизации</param>
        /// <returns>Санитизированная JSON строка</returns>
        public static string SanitizeForLogging(string json)
        {
            if (json.Length <= MaxLength)
                return json;

            // Проверяем, содержит ли JSON base64 данные (длинные строки без пробелов)
            if (
                json.Contains("\"data:image")
                || json.Contains("\"init_images\"")
                || json.Contains("\"mask\"")
            )
            {
                return $"[Request with image data, length: {json.Length} chars]";
            }

            // Обрезаем длинные JSON
#if NETSTANDARD2_0
            return json.Substring(0, MaxLength)
                + $"... [truncated, total length: {json.Length} chars]";
#else
            return json[..MaxLength] + $"... [truncated, total length: {json.Length} chars]";
#endif
        }
    }
}
