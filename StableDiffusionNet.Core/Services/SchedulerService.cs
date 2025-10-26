using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;

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
        public async Task<IReadOnlyList<Scheduler>> GetSchedulersAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available schedulers");

            var schedulers = await _httpClient.GetAsync<List<Scheduler>>(
                ApiEndpoints.Schedulers,
                cancellationToken
            );

            _logger.LogInformation($"Schedulers retrieved: {schedulers.Count}");

            return schedulers.AsReadOnly();
        }
    }
}
