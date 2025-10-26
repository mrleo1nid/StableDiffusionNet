using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Interfaces;
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
        private readonly ILogger<ImageToImageService> _logger;
        private const string Endpoint = "/sdapi/v1/img2img";

        /// <summary>
        /// Создает новый экземпляр сервиса генерации изображений из изображений
        /// </summary>
        public ImageToImageService(
            IHttpClientWrapper httpClient,
            ILogger<ImageToImageService> logger
        )
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

            if (request.InitImages == null || request.InitImages.Count == 0)
                throw new ArgumentException(
                    "At least one initial image must be provided",
                    nameof(request)
                );

            if (string.IsNullOrWhiteSpace(request.Prompt))
                throw new ArgumentException("Prompt cannot be empty", nameof(request));

            _logger.LogInformation(
                "Starting image-to-image generation. Prompt: {Prompt}, Denoising: {Denoising}",
                request.Prompt,
                request.DenoisingStrength
            );

            var response = await _httpClient.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                Endpoint,
                request,
                cancellationToken
            );

            _logger.LogInformation(
                "Generation completed. Images generated: {Count}",
                response.Images.Count
            );

            return response;
        }
    }
}
