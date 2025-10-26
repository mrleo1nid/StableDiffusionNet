using System;
using System.Net.Http;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Services;

namespace StableDiffusionNet
{
    /// <summary>
    /// Билдер для создания StableDiffusionClient без использования DI контейнера
    /// </summary>
    public class StableDiffusionClientBuilder
    {
        private StableDiffusionOptions _options = new StableDiffusionOptions();
        private IStableDiffusionLoggerFactory? _loggerFactory;
        private HttpClient? _httpClient;

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

            // Создаем wrapper для HttpClient
            // ownsHttpClient = true только если мы создали HttpClient сами (не был передан через WithHttpClient)
            var httpClientWrapper = new HttpClientWrapper(
                httpClient,
                loggerFactory.CreateLogger<HttpClientWrapper>(),
                _options,
                ownsHttpClient: _httpClient == null
            );

            // Создаем сервисы
            var textToImageService = new TextToImageService(
                httpClientWrapper,
                loggerFactory.CreateLogger<TextToImageService>()
            );

            var imageToImageService = new ImageToImageService(
                httpClientWrapper,
                loggerFactory.CreateLogger<ImageToImageService>()
            );

            var modelService = new ModelService(
                httpClientWrapper,
                loggerFactory.CreateLogger<ModelService>()
            );

            var progressService = new ProgressService(
                httpClientWrapper,
                loggerFactory.CreateLogger<ProgressService>()
            );

            var optionsService = new OptionsService(
                httpClientWrapper,
                loggerFactory.CreateLogger<OptionsService>()
            );

            var samplerService = new SamplerService(
                httpClientWrapper,
                loggerFactory.CreateLogger<SamplerService>()
            );

            var schedulerService = new SchedulerService(
                httpClientWrapper,
                loggerFactory.CreateLogger<SchedulerService>()
            );

            var upscalerService = new UpscalerService(
                httpClientWrapper,
                loggerFactory.CreateLogger<UpscalerService>()
            );

            var pngInfoService = new PngInfoService(
                httpClientWrapper,
                loggerFactory.CreateLogger<PngInfoService>()
            );

            var extraService = new ExtraService(
                httpClientWrapper,
                loggerFactory.CreateLogger<ExtraService>()
            );

            var embeddingService = new EmbeddingService(
                httpClientWrapper,
                loggerFactory.CreateLogger<EmbeddingService>()
            );

            var loraService = new LoraService(
                httpClientWrapper,
                loggerFactory.CreateLogger<LoraService>()
            );

            // Создаем главный клиент
            return new StableDiffusionClient(
                textToImageService,
                imageToImageService,
                modelService,
                progressService,
                optionsService,
                samplerService,
                schedulerService,
                upscalerService,
                pngInfoService,
                extraService,
                embeddingService,
                loraService,
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
