using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Helpers;
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
        private readonly ValidationOptions _validationOptions;

        /// <summary>
        /// Создает новый экземпляр сервиса генерации изображений из изображений
        /// </summary>
        public ImageToImageService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            Guard.ThrowIfNull(httpClient);
            Guard.ThrowIfNull(logger);
            Guard.ThrowIfNull(validationOptions);

            _httpClient = httpClient;
            _logger = logger;
            _validationOptions = validationOptions;
        }

        /// <inheritdoc/>
        public async Task<ImageToImageResponse> GenerateAsync(
            ImageToImageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            Guard.ThrowIfNull(request);
            request.Validate(_validationOptions, nameof(request));

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
