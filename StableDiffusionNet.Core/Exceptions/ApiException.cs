using System;
using System.Net;

namespace StableDiffusionNet.Exceptions
{
    /// <summary>
    /// Исключение при ошибке API
    /// Выбрасывается при ошибках HTTP коммуникации с Stable Diffusion API
    /// </summary>
    public class ApiException : StableDiffusionException
    {
        /// <summary>
        /// HTTP код ответа
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// Тело ответа
        /// </summary>
        public string? ResponseBody { get; }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <remarks>
        /// Используется для общих ошибок API без специфического HTTP статус кода
        /// </remarks>
        public ApiException(string message)
            : base(message) { }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и HTTP статус кодом
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="statusCode">HTTP статус код ошибки</param>
        /// <param name="responseBody">Тело ответа от API (опционально)</param>
        /// <remarks>
        /// Используется когда API возвращает ошибку с HTTP статус кодом.
        /// Статус код и тело ответа доступны через соответствующие свойства.
        /// </remarks>
        public ApiException(string message, HttpStatusCode statusCode, string? responseBody = null)
            : base($"{message} (Status: {statusCode})")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и внутренним исключением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="innerException">Внутреннее исключение, вызвавшее ошибку</param>
        /// <remarks>
        /// Используется для оборачивания исключений низкого уровня (например, HttpRequestException, JsonException)
        /// </remarks>
        public ApiException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
