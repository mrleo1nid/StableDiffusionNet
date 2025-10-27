using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
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
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса управления опциями
        /// </summary>
        public OptionsService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            Guard.ThrowIfNull(httpClient);
            Guard.ThrowIfNull(logger);

            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<WebUIOptions> GetOptionsAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting current WebUI options");

            var options = await _httpClient.GetAsync<WebUIOptions>(
                ApiEndpoints.Options,
                cancellationToken
            );

            _logger.LogDebug("Options successfully retrieved");

            return options;
        }

        /// <inheritdoc/>
        public async Task SetOptionsAsync(
            WebUIOptions options,
            CancellationToken cancellationToken = default
        )
        {
            Guard.ThrowIfNull(options);

            _logger.LogInformation("Setting WebUI options");

            await _httpClient.PostAsync(ApiEndpoints.Options, options, cancellationToken);

            _logger.LogInformation("Options successfully set");
        }
    }
}
