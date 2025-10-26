using System.Net;
using FluentAssertions;
using StableDiffusionNet.Exceptions;

namespace StableDiffusionNet.Tests.Exceptions
{
    /// <summary>
    /// Тесты для всех исключений библиотеки
    /// </summary>
    public class ExceptionsTests
    {
        #region StableDiffusionException Tests

        [Fact]
        public void StableDiffusionException_WithMessage_CreatesException()
        {
            // Arrange
            var message = "Test exception message";

            // Act
            var exception = new StableDiffusionException(message);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void StableDiffusionException_WithMessageAndInnerException_CreatesException()
        {
            // Arrange
            var message = "Test exception message";
            var innerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new StableDiffusionException(message, innerException);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.InnerException.Should().BeSameAs(innerException);
        }

        [Fact]
        public void StableDiffusionException_InheritsFromException()
        {
            // Arrange & Act
            var exception = new StableDiffusionException("test");

            // Assert
            exception.Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void StableDiffusionException_CanBeThrown()
        {
            // Arrange
            var message = "Error occurred";

            // Act
            Action act = () => throw new StableDiffusionException(message);

            // Assert
            act.Should().Throw<StableDiffusionException>().WithMessage(message);
        }

        #endregion

        #region ConfigurationException Tests

        [Fact]
        public void ConfigurationException_WithMessage_CreatesException()
        {
            // Arrange
            var message = "Configuration error";

            // Act
            var exception = new ConfigurationException(message);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void ConfigurationException_WithMessageAndInnerException_CreatesException()
        {
            // Arrange
            var message = "Configuration error";
            var innerException = new ArgumentException("Invalid argument");

            // Act
            var exception = new ConfigurationException(message, innerException);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.InnerException.Should().BeSameAs(innerException);
        }

        [Fact]
        public void ConfigurationException_InheritsFromStableDiffusionException()
        {
            // Arrange & Act
            var exception = new ConfigurationException("test");

            // Assert
            exception.Should().BeAssignableTo<StableDiffusionException>();
            exception.Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void ConfigurationException_CanBeThrown()
        {
            // Arrange
            var message = "BaseUrl cannot be empty";

            // Act
            Action act = () => throw new ConfigurationException(message);

            // Assert
            act.Should().Throw<ConfigurationException>().WithMessage(message);
        }

        [Fact]
        public void ConfigurationException_CanBeCaughtAsStableDiffusionException()
        {
            // Arrange
            var message = "Configuration error";

            // Act
            Action act = () => throw new ConfigurationException(message);

            // Assert
            act.Should().Throw<StableDiffusionException>().WithMessage(message);
        }

        #endregion

        #region ApiException Tests

        [Fact]
        public void ApiException_WithMessage_CreatesException()
        {
            // Arrange
            var message = "API error";

            // Act
            var exception = new ApiException(message);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.InnerException.Should().BeNull();
            exception.StatusCode.Should().BeNull();
            exception.ResponseBody.Should().BeNull();
        }

        [Fact]
        public void ApiException_WithMessageAndStatusCode_CreatesException()
        {
            // Arrange
            var message = "API error";
            var statusCode = HttpStatusCode.BadRequest;

            // Act
            var exception = new ApiException(message, statusCode);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Contain(message);
            exception.Message.Should().Contain(statusCode.ToString());
            exception.StatusCode.Should().Be(statusCode);
            exception.ResponseBody.Should().BeNull();
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void ApiException_WithMessageStatusCodeAndResponseBody_CreatesException()
        {
            // Arrange
            var message = "API error";
            var statusCode = HttpStatusCode.InternalServerError;
            var responseBody = "Server error details";

            // Act
            var exception = new ApiException(message, statusCode, responseBody);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Contain(message);
            exception.Message.Should().Contain(statusCode.ToString());
            exception.StatusCode.Should().Be(statusCode);
            exception.ResponseBody.Should().Be(responseBody);
        }

        [Fact]
        public void ApiException_WithMessageAndInnerException_CreatesException()
        {
            // Arrange
            var message = "API error";
            var innerException = new HttpRequestException("Connection failed");

            // Act
            var exception = new ApiException(message, innerException);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.InnerException.Should().BeSameAs(innerException);
            exception.StatusCode.Should().BeNull();
            exception.ResponseBody.Should().BeNull();
        }

        [Fact]
        public void ApiException_InheritsFromStableDiffusionException()
        {
            // Arrange & Act
            var exception = new ApiException("test");

            // Assert
            exception.Should().BeAssignableTo<StableDiffusionException>();
            exception.Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void ApiException_CanBeThrown()
        {
            // Arrange
            var message = "Request failed";
            var statusCode = HttpStatusCode.NotFound;

            // Act
            Action act = () => throw new ApiException(message, statusCode);

            // Assert
            act.Should().Throw<ApiException>().Which.StatusCode.Should().Be(statusCode);
        }

        [Fact]
        public void ApiException_CanBeCaughtAsStableDiffusionException()
        {
            // Arrange
            var message = "API error";

            // Act
            Action act = () => throw new ApiException(message);

            // Assert
            act.Should().Throw<StableDiffusionException>().WithMessage(message);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        public void ApiException_WithDifferentStatusCodes_StoresCorrectly(HttpStatusCode statusCode)
        {
            // Arrange & Act
            var exception = new ApiException("Test", statusCode);

            // Assert
            exception.StatusCode.Should().Be(statusCode);
            exception.Message.Should().Contain(statusCode.ToString());
        }

        [Fact]
        public void ApiException_WithNullResponseBody_HandlesCorrectly()
        {
            // Arrange & Act
            var exception = new ApiException("Test", HttpStatusCode.BadRequest, null);

            // Assert
            exception.ResponseBody.Should().BeNull();
        }

        [Fact]
        public void ApiException_WithEmptyResponseBody_HandlesCorrectly()
        {
            // Arrange & Act
            var exception = new ApiException("Test", HttpStatusCode.BadRequest, string.Empty);

            // Assert
            exception.ResponseBody.Should().BeEmpty();
        }

        [Fact]
        public void ApiException_WithLargeResponseBody_HandlesCorrectly()
        {
            // Arrange
            var largeBody = new string('x', 10000);

            // Act
            var exception = new ApiException("Test", HttpStatusCode.BadRequest, largeBody);

            // Assert
            exception.ResponseBody.Should().Be(largeBody);
            exception.ResponseBody.Should().HaveLength(10000);
        }

        [Fact]
        public void ApiException_MessageFormat_ContainsStatusCode()
        {
            // Arrange
            var message = "Request failed";
            var statusCode = HttpStatusCode.BadRequest;

            // Act
            var exception = new ApiException(message, statusCode);

            // Assert
            exception.Message.Should().Be($"{message} (Status: {statusCode})");
        }

        #endregion

        #region Exception Hierarchy Tests

        [Fact]
        public void ExceptionHierarchy_AllInheritCorrectly()
        {
            // Arrange & Act
            var baseException = new StableDiffusionException("base");
            var configException = new ConfigurationException("config");
            var apiException = new ApiException("api");

            // Assert
            baseException.Should().BeAssignableTo<Exception>();

            configException.Should().BeAssignableTo<StableDiffusionException>();
            configException.Should().BeAssignableTo<Exception>();

            apiException.Should().BeAssignableTo<StableDiffusionException>();
            apiException.Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void ExceptionHierarchy_CanCatchAllWithBaseType()
        {
            // Arrange
            Exception? caughtException = null;

            // Act
            try
            {
                throw new ApiException("test", HttpStatusCode.BadRequest);
            }
            catch (StableDiffusionException ex)
            {
                caughtException = ex;
            }

            // Assert
            caughtException.Should().NotBeNull();
            caughtException.Should().BeOfType<ApiException>();
        }

        [Fact]
        public void ExceptionHierarchy_ConfigurationExceptionDoesNotInheritFromApiException()
        {
            // Arrange & Act
            var exception = new ConfigurationException("test");

            // Assert
            exception.Should().NotBeAssignableTo<ApiException>();
        }

        #endregion
    }
}
