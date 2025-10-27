using System;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Результат проверки доступности API (health check).
    /// Содержит информацию о состоянии API и метрики производительности.
    /// </summary>
    public record HealthCheckResult
    {
        /// <summary>
        /// Указывает, доступен ли API
        /// </summary>
        public bool IsHealthy { get; init; }

        /// <summary>
        /// Время ответа API (response time).
        /// Null если произошла ошибка до получения ответа.
        /// </summary>
        public TimeSpan? ResponseTime { get; init; }

        /// <summary>
        /// Сообщение об ошибке, если API недоступен.
        /// Null если API работает корректно.
        /// </summary>
        public string? Error { get; init; }

        /// <summary>
        /// Временная метка выполнения проверки (UTC)
        /// </summary>
        public DateTime CheckedAt { get; init; }

        /// <summary>
        /// URL, который был проверен
        /// </summary>
        public string? Endpoint { get; init; }

        /// <summary>
        /// Создает успешный результат health check
        /// </summary>
        public static HealthCheckResult Success(TimeSpan responseTime, string? endpoint = null)
        {
            return new HealthCheckResult
            {
                IsHealthy = true,
                ResponseTime = responseTime,
                CheckedAt = DateTime.UtcNow,
                Endpoint = endpoint,
            };
        }

        /// <summary>
        /// Создает неуспешный результат health check
        /// </summary>
        public static HealthCheckResult Failure(string error, string? endpoint = null)
        {
            return new HealthCheckResult
            {
                IsHealthy = false,
                Error = error,
                CheckedAt = DateTime.UtcNow,
                Endpoint = endpoint,
            };
        }
    }
}
