using System;
using System.Net;

namespace StableDiffusionNet.Exceptions
{
    /// <summary>
    /// Исключение при ошибке API
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
        public ApiException(string message)
            : base(message) { }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и HTTP статус кодом
        /// </summary>
        public ApiException(string message, HttpStatusCode statusCode, string? responseBody = null)
            : base($"{message} (Status: {statusCode})")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и внутренним исключением
        /// </summary>
        public ApiException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
