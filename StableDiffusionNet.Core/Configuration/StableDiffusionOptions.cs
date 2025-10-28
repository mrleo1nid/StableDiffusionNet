using System;
using StableDiffusionNet.Exceptions;
using StableDiffusionNet.Helpers;

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
                ValidateBaseUrl(value, nameof(BaseUrl));
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
                ValidateTimeoutSeconds(value, nameof(TimeoutSeconds));
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
                ValidateRetryCount(value, nameof(RetryCount));
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
                ValidateRetryDelay(value, nameof(RetryDelayMilliseconds));
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
        /// Опции валидации (лимиты размеров изображений, длины JSON и т.д.)
        /// </summary>
        public ValidationOptions Validation { get; set; } = new ValidationOptions();

        /// <summary>
        /// Валидация настроек
        /// </summary>
        /// <remarks>
        /// Валидация выполняется автоматически в property setters.
        /// Этот метод полезен для явной проверки всех свойств после десериализации конфигурации,
        /// где setters могут не вызываться.
        /// </remarks>
        /// <exception cref="ConfigurationException">
        /// Выбрасывается, если BaseUrl пустой, невалидный,
        /// TimeoutSeconds не положительный, RetryCount отрицательный
        /// или RetryDelayMilliseconds отрицательный
        /// </exception>
        public void Validate()
        {
            // Используем те же методы валидации что и в property setters
            // Преобразуем ArgumentException в ConfigurationException для обратной совместимости
            try
            {
                ValidateBaseUrl(_baseUrl, nameof(BaseUrl));
                ValidateTimeoutSeconds(_timeoutSeconds, nameof(TimeoutSeconds));
                ValidateRetryCount(_retryCount, nameof(RetryCount));
                ValidateRetryDelay(_retryDelayMilliseconds, nameof(RetryDelayMilliseconds));

                // Валидация опций валидации
                Validation?.Validate();
            }
            catch (ArgumentException ex)
            {
                throw new ConfigurationException(ex.Message, ex);
            }
        }

        #region Private Validation Methods

        /// <summary>
        /// Централизованная валидация BaseUrl
        /// </summary>
        private static void ValidateBaseUrl(string value, string paramName)
        {
            Guard.ThrowIfNullOrWhiteSpaceWithName(value, paramName);

            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                throw new ArgumentException("BaseUrl must be a valid URL", paramName);
        }

        /// <summary>
        /// Централизованная валидация TimeoutSeconds
        /// </summary>
        private static void ValidateTimeoutSeconds(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException("TimeoutSeconds must be a positive number", paramName);
        }

        /// <summary>
        /// Централизованная валидация RetryCount
        /// </summary>
        private static void ValidateRetryCount(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException("RetryCount cannot be negative", paramName);
        }

        /// <summary>
        /// Централизованная валидация RetryDelayMilliseconds
        /// </summary>
        private static void ValidateRetryDelay(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException("RetryDelayMilliseconds cannot be negative", paramName);
        }

        #endregion
    }
}
