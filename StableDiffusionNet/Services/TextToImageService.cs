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
    /// Сервис для генерации изображений из текста (txt2img)
    /// Single Responsibility Principle - отвечает только за txt2img операции
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class TextToImageService : ITextToImageService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly ILogger<TextToImageService> _logger;
        private const string Endpoint = "/sdapi/v1/txt2img";

        /// <summary>
        /// Создает новый экземпляр сервиса генерации изображений из текста
        /// </summary>
        public TextToImageService(IHttpClientWrapper httpClient, ILogger<TextToImageService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<TextToImageResponse> GenerateAsync(
            TextToImageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Prompt))
                throw new ArgumentException("Prompt cannot be empty", nameof(request));

            _logger.LogInformation(
                "Starting text-to-image generation. Prompt: {Prompt}, Size: {Width}x{Height}",
                request.Prompt,
                request.Width,
                request.Height
            );

            var response = await _httpClient.PostAsync<TextToImageRequest, TextToImageResponse>(
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
