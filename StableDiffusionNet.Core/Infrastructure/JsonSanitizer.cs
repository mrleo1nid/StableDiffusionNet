using System;
using StableDiffusionNet.Configuration;

namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Санитизатор JSON для безопасного логирования.
    /// Single Responsibility - отвечает только за санитизацию JSON.
    /// Использует конфигурируемую максимальную длину вместо хардкоженной константы.
    /// </summary>
    internal class JsonSanitizer
    {
        private readonly ValidationOptions _validationOptions;

        /// <summary>
        /// Создает новый экземпляр санитизатора
        /// </summary>
        /// <param name="validationOptions">Опции валидации</param>
        public JsonSanitizer(ValidationOptions validationOptions)
        {
            _validationOptions =
                validationOptions ?? throw new ArgumentNullException(nameof(validationOptions));
        }

        /// <summary>
        /// Очищает JSON для безопасного логирования
        /// </summary>
        /// <param name="json">JSON строка для санитизации</param>
        /// <returns>Санитизированная JSON строка</returns>
        public string SanitizeForLogging(string json)
        {
            if (json.Length <= _validationOptions.MaxJsonLogLength)
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
            return json.Substring(0, _validationOptions.MaxJsonLogLength)
                + $"... [truncated, total length: {json.Length} chars]";
#else
            return json[.._validationOptions.MaxJsonLogLength]
                + $"... [truncated, total length: {json.Length} chars]";
#endif
        }
    }
}
