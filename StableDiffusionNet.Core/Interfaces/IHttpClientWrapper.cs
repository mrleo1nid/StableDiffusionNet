using System;
using System.Threading;
using System.Threading.Tasks;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для HTTP клиента
    /// Dependency Inversion Principle - абстракция для работы с HTTP
    /// Облегчает тестирование и замену реализации
    /// </summary>
    public interface IHttpClientWrapper : IDisposable
    {
        /// <summary>
        /// Выполняет GET запрос
        /// </summary>
        Task<TResponse> GetAsync<TResponse>(
            string endpoint,
            CancellationToken cancellationToken = default
        )
            where TResponse : class;

        /// <summary>
        /// Выполняет POST запрос
        /// </summary>
        Task<TResponse> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            CancellationToken cancellationToken = default
        )
            where TResponse : class;

        /// <summary>
        /// Выполняет POST запрос без тела
        /// </summary>
        Task PostAsync(string endpoint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполняет POST запрос с телом без ответа
        /// </summary>
        Task PostAsync<TRequest>(
            string endpoint,
            TRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
