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
    /// Сервис для постобработки изображений (апскейл, face restoration)
    /// Single Responsibility Principle - отвечает только за постобработку изображений
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class ExtraService : IExtraService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса постобработки изображений
        /// </summary>
        public ExtraService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ExtraSingleImageResponse> ProcessSingleImageAsync(
            ExtraSingleImageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogDebug("Processing single image with extras");

            var response = await _httpClient.PostAsync<
                ExtraSingleImageRequest,
                ExtraSingleImageResponse
            >(ApiEndpoints.ExtraSingleImage, request, cancellationToken);

            _logger.LogInformation("Single image processed successfully");

            return response;
        }
    }
}
