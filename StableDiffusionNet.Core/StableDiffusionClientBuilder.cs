using System;
using System.Net.Http;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;
using StableDiffusionNet.Services;

namespace StableDiffusionNet
{
    /// <summary>
    /// Билдер для создания StableDiffusionClient без использования DI контейнера
    /// </summary>
    /// <example>
    /// Базовое использование:
    /// <code>
    /// var client = new StableDiffusionClientBuilder()
    ///     .WithBaseUrl("http://localhost:7860")
    ///     .WithTimeout(300)
    ///     .Build();
    ///
    /// // Генерация изображения
    /// var request = new TextToImageRequest
    /// {
    ///     Prompt = "a beautiful sunset over mountains",
    ///     Width = 512,
    ///     Height = 512
    /// };
    ///
    /// var response = await client.TextToImage.GenerateAsync(request);
    /// </code>
    ///
    /// С настройкой повторных попыток и логированием:
    /// <code>
    /// var loggerFactory = new MyCustomLoggerFactory();
    ///
    /// var client = new StableDiffusionClientBuilder()
    ///     .WithBaseUrl("http://localhost:7860")
    ///     .WithRetry(retryCount: 3, retryDelayMilliseconds: 2000)
    ///     .WithLoggerFactory(loggerFactory)
    ///     .Build();
    /// </code>
    /// </example>
    public class StableDiffusionClientBuilder
    {
        private StableDiffusionOptions _options = new StableDiffusionOptions();
        private IStableDiffusionLoggerFactory? _loggerFactory;
        private HttpClient? _httpClient;
        private IStableDiffusionServiceFactory _serviceFactory = new DefaultServiceFactory();

        /// <summary>
        /// Устанавливает базовый URL для API
        /// </summary>
        /// <param name="baseUrl">Базовый URL (например: http://localhost:7860)</param>
        public StableDiffusionClientBuilder WithBaseUrl(string baseUrl)
        {
            _options.BaseUrl = baseUrl;
            return this;
        }

        /// <summary>
        /// Устанавливает таймаут для запросов
        /// </summary>
        /// <param name="timeoutSeconds">Таймаут в секундах</param>
        public StableDiffusionClientBuilder WithTimeout(int timeoutSeconds)
        {
            _options.TimeoutSeconds = timeoutSeconds;
            return this;
        }

        /// <summary>
        /// Устанавливает параметры повторных попыток
        /// </summary>
        /// <param name="retryCount">Количество попыток</param>
        /// <param name="retryDelayMilliseconds">Задержка между попытками в миллисекундах</param>
        public StableDiffusionClientBuilder WithRetry(
            int retryCount,
            int retryDelayMilliseconds = 1000
        )
        {
            _options.RetryCount = retryCount;
            _options.RetryDelayMilliseconds = retryDelayMilliseconds;
            return this;
        }

        /// <summary>
        /// Устанавливает API ключ
        /// </summary>
        /// <param name="apiKey">API ключ</param>
        public StableDiffusionClientBuilder WithApiKey(string apiKey)
        {
            _options.ApiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Включает детальное логирование
        /// </summary>
        public StableDiffusionClientBuilder WithDetailedLogging()
        {
            _options.EnableDetailedLogging = true;
            return this;
        }

        /// <summary>
        /// Устанавливает фабрику логгеров
        /// </summary>
        /// <param name="loggerFactory">Фабрика логгеров</param>
        public StableDiffusionClientBuilder WithLoggerFactory(
            IStableDiffusionLoggerFactory loggerFactory
        )
        {
            _loggerFactory = loggerFactory;
            return this;
        }

        /// <summary>
        /// Устанавливает кастомный HttpClient
        /// </summary>
        /// <param name="httpClient">HttpClient</param>
        public StableDiffusionClientBuilder WithHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            return this;
        }

        /// <summary>
        /// Устанавливает опции напрямую
        /// </summary>
        /// <param name="options">Опции</param>
        public StableDiffusionClientBuilder WithOptions(StableDiffusionOptions options)
        {
            Guard.ThrowIfNull(options);
            _options = options;
            return this;
        }

        /// <summary>
        /// Устанавливает фабрику для создания сервисов
        /// Позволяет переопределить стандартные реализации сервисов (полезно для тестирования)
        /// </summary>
        /// <param name="serviceFactory">Фабрика сервисов</param>
        /// <returns>Экземпляр Builder для цепочки вызовов</returns>
        /// <example>
        /// <code>
        /// var mockFactory = new Mock&lt;IStableDiffusionServiceFactory&gt;();
        /// var client = new StableDiffusionClientBuilder()
        ///     .WithBaseUrl("http://localhost:7860")
        ///     .WithServiceFactory(mockFactory.Object)
        ///     .Build();
        /// </code>
        /// </example>
        public StableDiffusionClientBuilder WithServiceFactory(
            IStableDiffusionServiceFactory serviceFactory
        )
        {
            Guard.ThrowIfNull(serviceFactory);
            _serviceFactory = serviceFactory;
            return this;
        }

        /// <summary>
        /// Создает экземпляр StableDiffusionClient
        /// </summary>
        /// <returns>Настроенный клиент</returns>
        public IStableDiffusionClient Build()
        {
            // Валидация опций
            _options.Validate();

            // Создаем логгер фабрику, если не предоставлена
            var loggerFactory = _loggerFactory ?? NullLoggerFactory.Instance;

            // Создаем HttpClient, если не предоставлен
            HttpClient httpClient;

            if (_httpClient != null)
            {
                httpClient = _httpClient;
            }
            else
            {
                // Создаем простой HttpClient
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri(_options.BaseUrl),
                    Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
                };

                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                {
                    httpClient.DefaultRequestHeaders.Add(
                        "Authorization",
                        $"Bearer {_options.ApiKey}"
                    );
                }
            }

            // Создаем wrapper для HttpClient с явным указанием владения
            // Если создали HttpClient сами (_httpClient == null), то wrapper владеет им и должен освободить
            // Если HttpClient был передан извне, то не владеем и не освобождаем
            var httpClientWrapper = new HttpClientWrapper(
                httpClient,
                loggerFactory.CreateLogger<HttpClientWrapper>(),
                _options,
                ownsHttpClient: _httpClient == null
            );

            // Создаем сервисы через фабрику (следуя Dependency Inversion Principle)
            var services = new StableDiffusionServices
            {
                TextToImage = _serviceFactory.CreateTextToImageService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<TextToImageService>(),
                    _options.Validation
                ),
                ImageToImage = _serviceFactory.CreateImageToImageService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<ImageToImageService>(),
                    _options.Validation
                ),
                Models = _serviceFactory.CreateModelService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<ModelService>(),
                    _options.Validation
                ),
                Progress = _serviceFactory.CreateProgressService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<ProgressService>(),
                    _options.Validation
                ),
                Options = _serviceFactory.CreateOptionsService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<OptionsService>(),
                    _options.Validation
                ),
                Samplers = _serviceFactory.CreateSamplerService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<SamplerService>(),
                    _options.Validation
                ),
                Schedulers = _serviceFactory.CreateSchedulerService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<SchedulerService>(),
                    _options.Validation
                ),
                Upscalers = _serviceFactory.CreateUpscalerService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<UpscalerService>(),
                    _options.Validation
                ),
                PngInfo = _serviceFactory.CreatePngInfoService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<PngInfoService>(),
                    _options.Validation
                ),
                Extra = _serviceFactory.CreateExtraService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<ExtraService>(),
                    _options.Validation
                ),
                Embeddings = _serviceFactory.CreateEmbeddingService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<EmbeddingService>(),
                    _options.Validation
                ),
                Loras = _serviceFactory.CreateLoraService(
                    httpClientWrapper,
                    loggerFactory.CreateLogger<LoraService>(),
                    _options.Validation
                ),
            };

            // Создаем главный клиент с упрощенным конструктором (Parameter Object Pattern)
            // HttpClient теперь управляется HttpClientWrapper (через ownsHttpClient флаг)
            // additionalDisposable больше не нужен для HttpClient
            return new StableDiffusionClient(
                services,
                httpClientWrapper,
                loggerFactory.CreateLogger<StableDiffusionClient>()
            );
        }

        /// <summary>
        /// Создает клиент с настройками по умолчанию
        /// </summary>
        /// <param name="baseUrl">Базовый URL API (если null, используется значение по умолчанию из StableDiffusionOptions)</param>
        /// <returns>Клиент с настройками по умолчанию</returns>
        public static IStableDiffusionClient CreateDefault(string? baseUrl = null)
        {
            var builder = new StableDiffusionClientBuilder();
            if (baseUrl != null)
            {
                builder.WithBaseUrl(baseUrl);
            }
            return builder.Build();
        }
    }
}
