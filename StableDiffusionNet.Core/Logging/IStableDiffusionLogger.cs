using System;

namespace StableDiffusionNet.Logging
{
    /// <summary>
    /// Минималистичный интерфейс для логирования в StableDiffusionNet.Core
    /// </summary>
    public interface IStableDiffusionLogger
    {
        /// <summary>
        /// Записывает сообщение с указанным уровнем
        /// </summary>
        /// <param name="logLevel">Уровень логирования</param>
        /// <param name="message">Сообщение</param>
        void Log(LogLevel logLevel, string message);

        /// <summary>
        /// Записывает сообщение с указанным уровнем и исключением
        /// </summary>
        /// <param name="logLevel">Уровень логирования</param>
        /// <param name="exception">Исключение</param>
        /// <param name="message">Сообщение</param>
        void Log(LogLevel logLevel, Exception exception, string message);

        /// <summary>
        /// Проверяет, включен ли указанный уровень логирования
        /// </summary>
        /// <param name="logLevel">Уровень логирования</param>
        /// <returns>true, если логирование включено для этого уровня</returns>
        bool IsEnabled(LogLevel logLevel);
    }

    /// <summary>
    /// Методы расширения для упрощения работы с логгером
    /// </summary>
    public static class StableDiffusionLoggerExtensions
    {
        /// <summary>
        /// Записывает отладочное сообщение
        /// </summary>
        public static void LogDebug(this IStableDiffusionLogger logger, string message)
        {
            logger.Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// Записывает информационное сообщение
        /// </summary>
        public static void LogInformation(this IStableDiffusionLogger logger, string message)
        {
            logger.Log(LogLevel.Information, message);
        }

        /// <summary>
        /// Записывает предупреждение
        /// </summary>
        public static void LogWarning(this IStableDiffusionLogger logger, string message)
        {
            logger.Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// Записывает сообщение об ошибке
        /// </summary>
        public static void LogError(this IStableDiffusionLogger logger, string message)
        {
            logger.Log(LogLevel.Error, message);
        }

        /// <summary>
        /// Записывает сообщение об ошибке с исключением
        /// </summary>
        public static void LogError(
            this IStableDiffusionLogger logger,
            Exception exception,
            string message
        )
        {
            logger.Log(LogLevel.Error, exception, message);
        }

        /// <summary>
        /// Записывает критическую ошибку
        /// </summary>
        public static void LogCritical(this IStableDiffusionLogger logger, string message)
        {
            logger.Log(LogLevel.Critical, message);
        }

        /// <summary>
        /// Записывает критическую ошибку с исключением
        /// </summary>
        public static void LogCritical(
            this IStableDiffusionLogger logger,
            Exception exception,
            string message
        )
        {
            logger.Log(LogLevel.Critical, exception, message);
        }
    }
}
