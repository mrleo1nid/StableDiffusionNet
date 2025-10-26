using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для получения информации о scheduler'ах (планировщиках)
    /// Single Responsibility Principle - отвечает только за операции с scheduler'ами
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class SchedulerService : ISchedulerService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса получения информации о scheduler'ах
        /// </summary>
        public SchedulerService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> GetSchedulersAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available schedulers");

            var schedulers = await _httpClient.GetAsync<JArray>(
                ApiEndpoints.Schedulers,
                cancellationToken
            );

            var schedulerNames = schedulers
                .Select(s => s["name"]?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name!)
                .ToList();

            _logger.LogInformation($"Schedulers retrieved: {schedulerNames.Count}");

            return schedulerNames.AsReadOnly();
        }
    }
}
