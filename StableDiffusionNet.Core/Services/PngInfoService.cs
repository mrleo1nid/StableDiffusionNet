using System;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для извлечения метаданных из PNG изображений
    /// Single Responsibility Principle - отвечает только за операции с PNG метаданными
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class PngInfoService : IPngInfoService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса извлечения PNG метаданных
        /// </summary>
        public PngInfoService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PngInfoResponse> GetPngInfoAsync(
            PngInfoRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogDebug("Extracting PNG info");

            var response = await _httpClient.PostAsync<PngInfoRequest, PngInfoResponse>(
                ApiEndpoints.PngInfo,
                request,
                cancellationToken
            );

            _logger.LogInformation("PNG info extracted successfully");

            return response;
        }
    }
}
