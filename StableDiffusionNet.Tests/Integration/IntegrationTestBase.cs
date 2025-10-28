using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.DependencyInjection.Extensions;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Базовый класс для интеграционных тестов
    /// Содержит общие настройки и утилиты
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly ServiceProvider ServiceProvider;
        protected readonly IStableDiffusionClient Client;
        private bool _disposed;

        protected IntegrationTestBase()
        {
            var services = new ServiceCollection();

            // Настройка клиента для интеграционных тестов
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.TimeoutSeconds = 300; // Уменьшаем таймаут для тестов
                options.EnableDetailedLogging = true;
                options.RetryCount = 2; // Уменьшаем количество попыток для тестов
                options.RetryDelayMilliseconds = 1000; // Быстрые повторы
            });

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            ServiceProvider = services.BuildServiceProvider();
            Client = ServiceProvider.GetRequiredService<IStableDiffusionClient>();
        }

        /// <summary>
        /// Проверяет доступность API перед запуском тестов
        /// </summary>
        protected async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var healthCheck = await Client.HealthCheckAsync();
                return healthCheck.IsHealthy;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            ServiceProvider?.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
