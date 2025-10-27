using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.DependencyInjection.Logging;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;
using StableDiffusionNet.Services;

namespace StableDiffusionNet.DependencyInjection.Extensions
{
    /// <summary>
    /// Расширения для регистрации StableDiffusionClient в DI контейнере
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет StableDiffusionClient в коллекцию сервисов
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configure">Действие для конфигурации опций</param>
        /// <returns>Коллекция сервисов для цепочки вызовов</returns>
        public static IServiceCollection AddStableDiffusion(
            this IServiceCollection services,
            Action<StableDiffusionOptions> configure
        )
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);
#else
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
#endif

            // Регистрация конфигурации
            services.Configure(configure);

            // Валидация конфигурации при старте
            services.AddSingleton<
                IValidateOptions<StableDiffusionOptions>,
                StableDiffusionOptionsValidator
            >();

            // Регистрация фабрики логгеров
            services.AddSingleton<IStableDiffusionLoggerFactory>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new MicrosoftLoggerFactory(loggerFactory);
            });

            // Константа для имени HttpClient
            const string httpClientName = "StableDiffusionHttpClient";

            // Регистрация HttpClient с настройкой
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(
                    (serviceProvider, client) =>
                    {
                        var options = serviceProvider
                            .GetRequiredService<IOptions<StableDiffusionOptions>>()
                            .Value;
                        client.BaseAddress = new Uri(options.BaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                        if (!string.IsNullOrWhiteSpace(options.ApiKey))
                        {
                            client.DefaultRequestHeaders.Add(
                                "Authorization",
                                $"Bearer {options.ApiKey}"
                            );
                        }
                    }
                );

            // Регистрация HttpClientWrapper с правильными зависимостями
            services.AddTransient<IHttpClientWrapper>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(httpClientName);

                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                var logger = loggerFactory.CreateLogger<HttpClientWrapper>();

                var options = sp.GetRequiredService<IOptions<StableDiffusionOptions>>().Value;

                // В DI сценарии HttpClient управляется IHttpClientFactory
                // Поэтому явно указываем ownsHttpClient: false
                return new HttpClientWrapper(httpClient, logger, options, ownsHttpClient: false);
            });

            // Регистрация сервисов с использованием Generic Helper метода (DRY принцип)
            // Text2Image и ImageToImage требуют ValidationOptions, поэтому регистрируются отдельно
            services.AddTransient<ITextToImageService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                var logger = loggerFactory.CreateLogger<TextToImageService>();
                var validationOptions = sp.GetRequiredService<
                    IOptions<StableDiffusionOptions>
                >().Value.Validation;

                return new TextToImageService(httpClient, logger, validationOptions);
            });

            services.AddTransient<IImageToImageService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                var logger = loggerFactory.CreateLogger<ImageToImageService>();
                var validationOptions = sp.GetRequiredService<
                    IOptions<StableDiffusionOptions>
                >().Value.Validation;

                return new ImageToImageService(httpClient, logger, validationOptions);
            });

            // Остальные сервисы не требуют ValidationOptions
            services.AddSdService<IModelService, ModelService>();
            services.AddSdService<IProgressService, ProgressService>();
            services.AddSdService<IOptionsService, OptionsService>();
            services.AddSdService<ISamplerService, SamplerService>();
            services.AddSdService<ISchedulerService, SchedulerService>();
            services.AddSdService<IUpscalerService, UpscalerService>();
            services.AddSdService<IPngInfoService, PngInfoService>();
            services.AddSdService<IExtraService, ExtraService>();
            services.AddSdService<IEmbeddingService, EmbeddingService>();
            services.AddSdService<ILoraService, LoraService>();

            // Регистрация главного клиента с использованием Parameter Object Pattern
            services.AddTransient<IStableDiffusionClient>(sp =>
            {
                var sdServices = new StableDiffusionServices
                {
                    TextToImage = sp.GetRequiredService<ITextToImageService>(),
                    ImageToImage = sp.GetRequiredService<IImageToImageService>(),
                    Models = sp.GetRequiredService<IModelService>(),
                    Progress = sp.GetRequiredService<IProgressService>(),
                    Options = sp.GetRequiredService<IOptionsService>(),
                    Samplers = sp.GetRequiredService<ISamplerService>(),
                    Schedulers = sp.GetRequiredService<ISchedulerService>(),
                    Upscalers = sp.GetRequiredService<IUpscalerService>(),
                    PngInfo = sp.GetRequiredService<IPngInfoService>(),
                    Extra = sp.GetRequiredService<IExtraService>(),
                    Embeddings = sp.GetRequiredService<IEmbeddingService>(),
                    Loras = sp.GetRequiredService<ILoraService>(),
                };

                var httpClientWrapper = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();

                return new StableDiffusionClient(
                    sdServices,
                    httpClientWrapper,
                    loggerFactory.CreateLogger<StableDiffusionClient>()
                );
            });

            return services;
        }

        /// <summary>
        /// Добавляет StableDiffusionClient в коллекцию сервисов с опциями по умолчанию
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="baseUrl">Базовый URL API (если null, используется значение по умолчанию из StableDiffusionOptions)</param>
        /// <returns>Коллекция сервисов для цепочки вызовов</returns>
        public static IServiceCollection AddStableDiffusion(
            this IServiceCollection services,
            string? baseUrl = null
        )
        {
            return services.AddStableDiffusion(options =>
            {
                if (baseUrl != null)
                {
                    options.BaseUrl = baseUrl;
                }
            });
        }

        /// <summary>
        /// Generic метод для регистрации Stable Diffusion сервисов в DI контейнере.
        /// Применяет DRY принцип для устранения дублирования кода.
        /// </summary>
        /// <typeparam name="TInterface">Тип интерфейса сервиса</typeparam>
        /// <typeparam name="TImplementation">Тип реализации сервиса</typeparam>
        /// <param name="services">Коллекция сервисов</param>
        /// <returns>Коллекция сервисов для цепочки вызовов</returns>
        /// <remarks>
        /// Все Stable Diffusion сервисы имеют одинаковую структуру конструктора:
        /// (IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        /// Этот метод автоматически создает экземпляры с правильными зависимостями.
        /// </remarks>
        private static IServiceCollection AddSdService<TInterface, TImplementation>(
            this IServiceCollection services
        )
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddTransient<TInterface>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                var logger = loggerFactory.CreateLogger<TImplementation>();

                // Используем Activator для создания экземпляра с двумя параметрами
                return (TImplementation)
                    Activator.CreateInstance(typeof(TImplementation), httpClient, logger)!;
            });

            return services;
        }
    }

    /// <summary>
    /// Валидатор для StableDiffusionOptions
    /// </summary>
    internal class StableDiffusionOptionsValidator : IValidateOptions<StableDiffusionOptions>
    {
        public ValidateOptionsResult Validate(string? name, StableDiffusionOptions options)
        {
            try
            {
                options.Validate();
                return ValidateOptionsResult.Success;
            }
            catch (Exception ex)
            {
                return ValidateOptionsResult.Fail(ex.Message);
            }
        }
    }
}
