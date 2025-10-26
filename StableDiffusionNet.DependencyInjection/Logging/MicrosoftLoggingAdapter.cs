using System;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Logging;
using MsftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using SdLogLevel = StableDiffusionNet.Logging.LogLevel;

namespace StableDiffusionNet.DependencyInjection.Logging
{
    /// <summary>
    /// Адаптер для интеграции Microsoft.Extensions.Logging с StableDiffusionNet.Core логированием
    /// </summary>
    internal class MicrosoftLoggingAdapter : IStableDiffusionLogger
    {
        private readonly ILogger _logger;

        public MicrosoftLoggingAdapter(ILogger logger)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(logger);
#else
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
#endif
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Log(SdLogLevel logLevel, string message)
        {
            var msftLogLevel = ConvertLogLevel(logLevel);
            _logger.Log(msftLogLevel, "{Message}", message);
        }

        /// <inheritdoc/>
        public void Log(SdLogLevel logLevel, Exception exception, string message)
        {
            var msftLogLevel = ConvertLogLevel(logLevel);
            _logger.Log(msftLogLevel, exception, "{Message}", message);
        }

        /// <inheritdoc/>
        public bool IsEnabled(SdLogLevel logLevel)
        {
            var msftLogLevel = ConvertLogLevel(logLevel);
            return _logger.IsEnabled(msftLogLevel);
        }

        /// <summary>
        /// Преобразует уровень логирования из StableDiffusionNet в Microsoft.Extensions.Logging
        /// </summary>
        private static MsftLogLevel ConvertLogLevel(SdLogLevel logLevel)
        {
            return logLevel switch
            {
                SdLogLevel.Debug => MsftLogLevel.Debug,
                SdLogLevel.Information => MsftLogLevel.Information,
                SdLogLevel.Warning => MsftLogLevel.Warning,
                SdLogLevel.Error => MsftLogLevel.Error,
                SdLogLevel.Critical => MsftLogLevel.Critical,
                _ => MsftLogLevel.Information,
            };
        }
    }
}
