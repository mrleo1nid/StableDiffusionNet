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

                return new HttpClientWrapper(httpClient, logger, options);
            });

            // Регистрация сервисов
            services.AddTransient<ITextToImageService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new TextToImageService(
                    httpClient,
                    loggerFactory.CreateLogger<TextToImageService>()
                );
            });

            services.AddTransient<IImageToImageService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new ImageToImageService(
                    httpClient,
                    loggerFactory.CreateLogger<ImageToImageService>()
                );
            });

            services.AddTransient<IModelService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new ModelService(httpClient, loggerFactory.CreateLogger<ModelService>());
            });

            services.AddTransient<IProgressService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new ProgressService(
                    httpClient,
                    loggerFactory.CreateLogger<ProgressService>()
                );
            });

            services.AddTransient<IOptionsService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new OptionsService(httpClient, loggerFactory.CreateLogger<OptionsService>());
            });

            services.AddTransient<ISamplerService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new SamplerService(httpClient, loggerFactory.CreateLogger<SamplerService>());
            });

            services.AddTransient<ISchedulerService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new SchedulerService(
                    httpClient,
                    loggerFactory.CreateLogger<SchedulerService>()
                );
            });

            services.AddTransient<IUpscalerService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new UpscalerService(
                    httpClient,
                    loggerFactory.CreateLogger<UpscalerService>()
                );
            });

            services.AddTransient<IPngInfoService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new PngInfoService(httpClient, loggerFactory.CreateLogger<PngInfoService>());
            });

            services.AddTransient<IExtraService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new ExtraService(httpClient, loggerFactory.CreateLogger<ExtraService>());
            });

            services.AddTransient<IEmbeddingService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new EmbeddingService(
                    httpClient,
                    loggerFactory.CreateLogger<EmbeddingService>()
                );
            });

            services.AddTransient<ILoraService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientWrapper>();
                var loggerFactory = sp.GetRequiredService<IStableDiffusionLoggerFactory>();
                return new LoraService(httpClient, loggerFactory.CreateLogger<LoraService>());
            });

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
