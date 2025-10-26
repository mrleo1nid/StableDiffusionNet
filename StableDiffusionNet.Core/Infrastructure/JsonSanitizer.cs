using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Санитизатор JSON для безопасного логирования
    /// Single Responsibility - отвечает только за санитизацию JSON
    /// </summary>
    internal class JsonSanitizer : IJsonSanitizer
    {
        private const int MaxLength = 500;

        /// <inheritdoc/>
        public string SanitizeForLogging(string json)
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
