using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Exceptions;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests.Infrastructure.HttpClientWrapperTests
{
    /// <summary>
    /// Тесты для HttpClientWrapper - конструктор и жизненный цикл (Dispose)
    /// </summary>
    public class HttpClientWrapperLifecycleTests
    {
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly StableDiffusionOptions _options;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public HttpClientWrapperLifecycleTests()
        {
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _options = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                TimeoutSeconds = 300,
                EnableDetailedLogging = false,
            };
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        private HttpClientWrapper CreateWrapper()
        {
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                // HttpClient должен быть настроен (обычно это делается в ServiceCollectionExtensions)
                BaseAddress = new Uri(_options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };
            return new HttpClientWrapper(httpClient, _loggerMock.Object, _options);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Arrange
            // Act & Assert
            var act = () => new HttpClientWrapper(null!, _loggerMock.Object, _options);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();
            // Act & Assert
            var act = () => new HttpClientWrapper(httpClient, null!, _options);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            var act = () => new HttpClientWrapper(httpClient, _loggerMock.Object, null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithInvalidOptions_ThrowsConfigurationException()
        {
            // Act & Assert - валидация теперь происходит в setter
            var act = () => new StableDiffusionOptions { BaseUrl = string.Empty };
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*")
                .WithParameterName("BaseUrl");
        }

        [Fact]
        public void Constructor_WithDefaultOwnsHttpClient_IsFalse()
        {
            // Arrange & Act
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri(_options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };

            var wrapper = new HttpClientWrapper(httpClient, _loggerMock.Object, _options);

            // Assert - HttpClient не должен быть освобожден при Dispose wrapper'а
            wrapper.Dispose();

            // HttpClient должен остаться работоспособным
            Action act = () => httpClient.BaseAddress.Should().NotBeNull();
            act.Should().NotThrow("HttpClient не должен быть освобожден wrapper'ом по умолчанию");

            // Cleanup
            httpClient.Dispose();
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var wrapper = CreateWrapper();

            // Act & Assert
            var act = () =>
            {
                wrapper.Dispose();
#pragma warning disable IDE0079, CA1816 // Remove unnecessary suppression
#pragma warning disable IDISP016, IDISP017, S3966 // Don't use disposed object, Don't use disposed member
                wrapper.Dispose(); // Вторая попытка не должна бросать исключение (тестируем идемпотентность Dispose)
#pragma warning restore IDISP016, IDISP017,S3966
#pragma warning restore IDE0079, CA1816
            };
            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_CanBeCalledSafely()
        {
            // Arrange
            var wrapper = CreateWrapper();

            // Act & Assert
            var act = () => wrapper.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_WithOwnsHttpClientFalse_DoesNotDisposeHttpClient()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri(_options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };

            // Создаем wrapper с ownsHttpClient: false (по умолчанию)
            var wrapper = new HttpClientWrapper(
                httpClient,
                _loggerMock.Object,
                _options,
                ownsHttpClient: false
            );

            // Act
            wrapper.Dispose();

            // Assert - HttpClient должен остаться работоспособным
            Action act = () => httpClient.BaseAddress.Should().NotBeNull();
            act.Should().NotThrow("HttpClient не должен быть освобожден wrapper'ом");

            // Cleanup
            httpClient.Dispose();
        }

        [Fact]
        public void Dispose_WithOwnsHttpClientTrue_DisposesHttpClient()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri(_options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };

            // Создаем wrapper с ownsHttpClient: true
            var wrapper = new HttpClientWrapper(
                httpClient,
                _loggerMock.Object,
                _options,
                ownsHttpClient: true
            );

            // Act
            wrapper.Dispose();

            // Assert - HttpClient должен быть освобожден
#pragma warning disable IDISP013 // Await in using
            Action act = () => httpClient.GetAsync("http://test");
            act.Should()
                .Throw<ObjectDisposedException>("HttpClient должен быть освобожден wrapper'ом");
#pragma warning restore IDISP013
        }

        #endregion
    }
}
