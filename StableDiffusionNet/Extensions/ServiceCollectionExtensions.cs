using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Services;

namespace StableDiffusionNet.Extensions
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

            // Регистрация HttpClient с Polly retry policy
            services
                .AddHttpClient<IHttpClientWrapper, HttpClientWrapper>()
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
                )
                .AddPolicyHandler(
                    (serviceProvider, request) =>
                    {
                        var options = serviceProvider
                            .GetRequiredService<IOptions<StableDiffusionOptions>>()
                            .Value;

                        // Retry policy для транзитных HTTP ошибок
                        var retryPolicy = HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .WaitAndRetryAsync(
                                options.RetryCount,
                                retryAttempt =>
                                    TimeSpan.FromMilliseconds(
                                        options.RetryDelayMilliseconds * retryAttempt
                                    )
                            );

                        // Специальная обработка HTTP 429 (Too Many Requests)
                        var rateLimitPolicy = Policy
                            .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode == 429)
                            .WaitAndRetryAsync(
                                options.RetryCount,
                                retryAttempt =>
                                {
                                    // Увеличенная задержка для rate limiting
                                    var baseDelay = options.RetryDelayMilliseconds * 2;
                                    return TimeSpan.FromMilliseconds(baseDelay * retryAttempt);
                                }
                            );

                        // Комбинируем политики
                        return Policy.WrapAsync(rateLimitPolicy, retryPolicy);
                    }
                );

            // Регистрация сервисов
            services.AddTransient<ITextToImageService, TextToImageService>();
            services.AddTransient<IImageToImageService, ImageToImageService>();
            services.AddTransient<IModelService, ModelService>();
            services.AddTransient<IProgressService, ProgressService>();
            services.AddTransient<IOptionsService, OptionsService>();
            services.AddTransient<ISamplerService, SamplerService>();

            // Регистрация главного клиента
            services.AddTransient<IStableDiffusionClient, StableDiffusionClient>();

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
