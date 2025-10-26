using System;

namespace StableDiffusionNet.Exceptions
{
    /// <summary>
    /// Исключение при ошибке конфигурации
    /// Выбрасывается при невалидных настройках клиента (например, пустой BaseUrl, отрицательный Timeout и т.д.)
    /// </summary>
    public class ConfigurationException : StableDiffusionException
    {
        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением
        /// </summary>
        /// <param name="message">Сообщение об ошибке конфигурации</param>
        /// <remarks>
        /// Выбрасывается методом StableDiffusionOptions.Validate() при обнаружении невалидных настроек
        /// </remarks>
        public ConfigurationException(string message)
            : base(message) { }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и внутренним исключением
        /// </summary>
        /// <param name="message">Сообщение об ошибке конфигурации</param>
        /// <param name="innerException">Внутреннее исключение, вызвавшее ошибку</param>
        public ConfigurationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
