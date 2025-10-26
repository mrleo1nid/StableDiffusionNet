using System;
using StableDiffusionNet.Exceptions;

namespace StableDiffusionNet.Configuration
{
    /// <summary>
    /// Конфигурация для клиента Stable Diffusion WebUI API
    /// </summary>
    public class StableDiffusionOptions
    {
        /// <summary>
        /// Базовый URL API (например: http://localhost:7860)
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:7860";

        /// <summary>
        /// Таймаут для запросов в секундах
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Количество попыток повтора при ошибке
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Задержка между повторами в миллисекундах
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 1000;

        /// <summary>
        /// API ключ (если требуется)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Включить детальное логирование
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;

        /// <summary>
        /// Валидация настроек
        /// </summary>
        /// <exception cref="ConfigurationException">
        /// Выбрасывается, если BaseUrl пустой, невалидный,
        /// TimeoutSeconds не положительный, RetryCount отрицательный
        /// или RetryDelayMilliseconds отрицательный
        /// </exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
                throw new ConfigurationException("BaseUrl cannot be empty");

            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
                throw new ConfigurationException("BaseUrl must be a valid URL");

            if (TimeoutSeconds <= 0)
                throw new ConfigurationException("TimeoutSeconds must be a positive number");

            if (RetryCount < 0)
                throw new ConfigurationException("RetryCount cannot be negative");

            if (RetryDelayMilliseconds < 0)
                throw new ConfigurationException("RetryDelayMilliseconds cannot be negative");
        }
    }
}
