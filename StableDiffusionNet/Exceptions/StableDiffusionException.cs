using System;

namespace StableDiffusionNet.Exceptions
{
    /// <summary>
    /// Базовое исключение для всех ошибок Stable Diffusion API
    /// </summary>
    public class StableDiffusionException : Exception
    {
        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением
        /// </summary>
        public StableDiffusionException(string message)
            : base(message) { }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и внутренним исключением
        /// </summary>
        public StableDiffusionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
