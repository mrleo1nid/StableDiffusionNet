using System;

namespace StableDiffusionNet.Exceptions
{
    /// <summary>
    /// Базовое исключение для всех ошибок Stable Diffusion API
    /// </summary>
    /// <remarks>
    /// Все специфические исключения библиотеки наследуются от этого класса.
    /// Позволяет перехватывать все ошибки библиотеки одним catch блоком.
    /// Производные исключения:
    /// - <see cref="ApiException"/> - ошибки HTTP коммуникации
    /// - <see cref="ConfigurationException"/> - ошибки конфигурации
    /// </remarks>
    public class StableDiffusionException : Exception
    {
        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public StableDiffusionException(string message)
            : base(message) { }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и внутренним исключением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="innerException">Внутреннее исключение, вызвавшее ошибку</param>
        public StableDiffusionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
