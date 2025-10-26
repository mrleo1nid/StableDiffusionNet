using System;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.DependencyInjection.Logging
{
    /// <summary>
    /// Фабрика для создания адаптеров логгеров Microsoft.Extensions.Logging
    /// </summary>
    internal class MicrosoftLoggerFactory : IStableDiffusionLoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public MicrosoftLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory =
                loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <inheritdoc/>
        public IStableDiffusionLogger CreateLogger<T>()
        {
            var logger = _loggerFactory.CreateLogger<T>();
            return new MicrosoftLoggingAdapter(logger);
        }

        /// <inheritdoc/>
        public IStableDiffusionLogger CreateLogger(string categoryName)
        {
            var logger = _loggerFactory.CreateLogger(categoryName);
            return new MicrosoftLoggingAdapter(logger);
        }
    }
}
