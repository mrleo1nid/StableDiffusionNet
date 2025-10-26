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
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

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

                return new HttpClientWrapper(httpClient, logger, options);
            });

            // Регистрация сервисов
            RegisterService<ITextToImageService, TextToImageService>(services);
            RegisterService<IImageToImageService, ImageToImageService>(services);
            RegisterService<IModelService, ModelService>(services);
            RegisterService<IProgressService, ProgressService>(services);
            RegisterService<IOptionsService, OptionsService>(services);
            RegisterService<ISamplerService, SamplerService>(services);
            RegisterService<ISchedulerService, SchedulerService>(services);
            RegisterService<IUpscalerService, UpscalerService>(services);
            RegisterService<IPngInfoService, PngInfoService>(services);
            RegisterService<IExtraService, ExtraService>(services);
            RegisterService<IEmbeddingService, EmbeddingService>(services);
            RegisterService<ILoraService, LoraService>(services);

            // Регистрация главного клиента
            services.AddTransient<IStableDiffusionClient>(sp =>
            {
                var textToImage = sp.GetRequiredService<ITextToImageService>();
                var imageToImage = sp.GetRequiredService<IImageToImageService>();
                var models = sp.GetRequiredService<IModelService>();
                var progress = sp.GetRequiredService<IProgressService>();
                var options = sp.GetRequiredService<IOptionsService>();
                var samplers = sp.GetRequiredService<ISamplerService>();
                var schedulers = sp.GetRequiredService<ISchedulerService>();
                var upscalers = sp.GetRequiredService<IUpscalerService>();
                var pngInfo = sp.GetRequiredService<IPngInfoService>();
                var extra = sp.GetRequiredService<IExtraService>();
                var embeddings = sp.GetRequiredService<IEmbeddingService>();
                var loras = sp.GetRequiredService<ILoraService>();
                var httpClientWrapper = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();

                return new StableDiffusionClient(
                    textToImage,
                    imageToImage,
                    models,
                    progress,
                    options,
                    samplers,
                    schedulers,
                    upscalers,
                    pngInfo,
                    extra,
                    embeddings,
                    loras,
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
        /// <param name="baseUrl">Базовый URL API (например: http://localhost:7860)</param>
        /// <returns>Коллекция сервисов для цепочки вызовов</returns>
        public static IServiceCollection AddStableDiffusion(
            this IServiceCollection services,
            string baseUrl = "http://localhost:7860"
        )
        {
            return services.AddStableDiffusion(options =>
            {
                options.BaseUrl = baseUrl;
            });
        }

        /// <summary>
        /// Регистрирует сервис в DI контейнере
        /// </summary>
        private static void RegisterService<TInterface, TImplementation>(
            IServiceCollection services
        )
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddTransient<TInterface>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return (TInterface)
                    Activator.CreateInstance(
                        typeof(TImplementation),
                        httpClient,
                        loggerFactory.CreateLogger<TImplementation>()
                    )!;
            });
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
