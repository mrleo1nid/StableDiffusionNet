using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для управления опциями WebUI
    /// Single Responsibility Principle - отвечает только за операции с опциями
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class OptionsService : IOptionsService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly ILogger<OptionsService> _logger;
        private const string Endpoint = "/sdapi/v1/options";

        /// <summary>
        /// Создает новый экземпляр сервиса управления опциями
        /// </summary>
        public OptionsService(IHttpClientWrapper httpClient, ILogger<OptionsService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<WebUIOptions> GetOptionsAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Получение текущих опций WebUI");

            var options = await _httpClient.GetAsync<WebUIOptions>(Endpoint, cancellationToken);

            _logger.LogDebug("Опции успешно получены");

            return options;
        }

        /// <inheritdoc/>
        public async Task SetOptionsAsync(
            WebUIOptions options,
            CancellationToken cancellationToken = default
        )
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _logger.LogInformation("Установка опций WebUI");

            await _httpClient.PostAsync(Endpoint, options, cancellationToken);

            _logger.LogInformation("Опции успешно установлены");
        }
    }
}
