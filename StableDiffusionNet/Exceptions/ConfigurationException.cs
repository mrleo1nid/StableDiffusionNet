using System;

namespace StableDiffusionNet.Exceptions
{
    /// <summary>
    /// Исключение при ошибке конфигурации
    /// </summary>
    public class ConfigurationException : StableDiffusionException
    {
        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением
        /// </summary>
        public ConfigurationException(string message)
            : base(message) { }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и внутренним исключением
        /// </summary>
        public ConfigurationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
