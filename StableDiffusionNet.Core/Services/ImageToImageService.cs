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
    /// Сервис для генерации изображений из изображений (img2img)
    /// Single Responsibility Principle - отвечает только за img2img операции
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class ImageToImageService : IImageToImageService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса генерации изображений из изображений
        /// </summary>
        public ImageToImageService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ImageToImageResponse> GenerateAsync(
            ImageToImageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Validate(nameof(request));

            _logger.LogInformation(
                $"Starting image-to-image generation. Prompt: {request.Prompt}, Denoising: {request.DenoisingStrength}"
            );

            var response = await _httpClient.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                ApiEndpoints.ImageToImage,
                request,
                cancellationToken
            );

            _logger.LogInformation(
                $"Generation completed. Images generated: {response.Images?.Count ?? 0}"
            );

            return response;
        }
    }
}
