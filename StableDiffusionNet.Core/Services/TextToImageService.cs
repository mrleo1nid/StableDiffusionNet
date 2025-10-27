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
    /// Сервис для генерации изображений из текста (txt2img)
    /// Single Responsibility Principle - отвечает только за txt2img операции
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class TextToImageService : ITextToImageService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;
        private readonly ValidationOptions _validationOptions;

        /// <summary>
        /// Создает новый экземпляр сервиса генерации изображений из текста
        /// </summary>
        public TextToImageService(
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
        public async Task<TextToImageResponse> GenerateAsync(
            TextToImageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            Guard.ThrowIfNull(request);
            request.Validate(_validationOptions, nameof(request));

            _logger.LogInformation(
                $"Starting text-to-image generation. Prompt: {request.Prompt}, Size: {request.Width}x{request.Height}"
            );

            var response = await _httpClient.PostAsync<TextToImageRequest, TextToImageResponse>(
                ApiEndpoints.TextToImage,
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
