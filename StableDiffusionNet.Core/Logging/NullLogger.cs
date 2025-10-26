using System;

namespace StableDiffusionNet.Logging
{
    /// <summary>
    /// Пустая реализация логгера, которая не выполняет никаких действий
    /// </summary>
    public sealed class NullLogger : IStableDiffusionLogger
    {
        /// <summary>
        /// Единственный экземпляр NullLogger
        /// </summary>
        public static readonly NullLogger Instance = new NullLogger();

        private NullLogger() { }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, string message)
        {
            // Ничего не делаем
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, Exception exception, string message)
        {
            // Ничего не делаем
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }
    }
}
