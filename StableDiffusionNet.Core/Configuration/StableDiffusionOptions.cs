using System;
using StableDiffusionNet.Exceptions;

namespace StableDiffusionNet.Configuration
{
    /// <summary>
    /// Конфигурация для клиента Stable Diffusion WebUI API
    /// </summary>
    public class StableDiffusionOptions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Security",
            "S1075:URIs should not be hardcoded",
            Justification = "Default localhost URL for Stable Diffusion WebUI API - configurable via BaseUrl property"
        )]
        private string _baseUrl = "http://localhost:7860";
        private int _timeoutSeconds = 300;
        private int _retryCount = 3;
        private int _retryDelayMilliseconds = 1000;

        /// <summary>
        /// Базовый URL API (например: http://localhost:7860)
        /// </summary>
        /// <exception cref="ArgumentException">Выбрасывается если URL пустой или невалидный</exception>
        public string BaseUrl
        {
            get => _baseUrl;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("BaseUrl cannot be empty", nameof(BaseUrl));

                if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    throw new ArgumentException("BaseUrl must be a valid URL", nameof(BaseUrl));

                _baseUrl = value;
            }
        }

        /// <summary>
        /// Таймаут для запросов в секундах
        /// </summary>
        /// <exception cref="ArgumentException">Выбрасывается если значение не положительное</exception>
        public int TimeoutSeconds
        {
            get => _timeoutSeconds;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(
                        "TimeoutSeconds must be a positive number",
                        nameof(TimeoutSeconds)
                    );

                _timeoutSeconds = value;
            }
        }

        /// <summary>
        /// Количество попыток повтора при ошибке
        /// </summary>
        /// <exception cref="ArgumentException">Выбрасывается если значение отрицательное</exception>
        public int RetryCount
        {
            get => _retryCount;
            set
            {
                if (value < 0)
                    throw new ArgumentException(
                        "RetryCount cannot be negative",
                        nameof(RetryCount)
                    );

                _retryCount = value;
            }
        }

        /// <summary>
        /// Задержка между повторами в миллисекундах
        /// </summary>
        /// <exception cref="ArgumentException">Выбрасывается если значение отрицательное</exception>
        public int RetryDelayMilliseconds
        {
            get => _retryDelayMilliseconds;
            set
            {
                if (value < 0)
                    throw new ArgumentException(
                        "RetryDelayMilliseconds cannot be negative",
                        nameof(RetryDelayMilliseconds)
                    );

                _retryDelayMilliseconds = value;
            }
        }

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
        /// <remarks>
        /// Начиная с версии, где валидация выполняется в property setters,
        /// этот метод оставлен для обратной совместимости.
        /// Все проверки теперь выполняются автоматически при установке свойств.
        /// </remarks>
        /// <exception cref="ConfigurationException">
        /// Выбрасывается, если BaseUrl пустой, невалидный,
        /// TimeoutSeconds не положительный, RetryCount отрицательный
        /// или RetryDelayMilliseconds отрицательный
        /// </exception>
        public void Validate()
        {
            // Валидация теперь выполняется в property setters
            // Этот метод оставлен для обратной совместимости и для явной проверки
            // всех свойств после десериализации конфигурации (где setters могут не вызываться)

            // Проверяем значения напрямую, используя ConfigurationException для совместимости
            if (string.IsNullOrWhiteSpace(_baseUrl))
                throw new ConfigurationException("BaseUrl cannot be empty");

            if (!Uri.TryCreate(_baseUrl, UriKind.Absolute, out _))
                throw new ConfigurationException("BaseUrl must be a valid URL");

            if (_timeoutSeconds <= 0)
                throw new ConfigurationException("TimeoutSeconds must be a positive number");

            if (_retryCount < 0)
                throw new ConfigurationException("RetryCount cannot be negative");

            if (_retryDelayMilliseconds < 0)
                throw new ConfigurationException("RetryDelayMilliseconds cannot be negative");
        }
    }
}
