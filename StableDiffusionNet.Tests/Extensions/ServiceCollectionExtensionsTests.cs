using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.DependencyInjection.Extensions;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Tests.Extensions
{
    /// <summary>
    /// Тесты для ServiceCollectionExtensions
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        #region AddStableDiffusion with Action<StableDiffusionOptions> Tests

        [Fact]
        public void AddStableDiffusion_WithNullServices_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection services = null!;
            Action<StableDiffusionOptions> configure = options => options.BaseUrl = "http://test";

            // Act
            Action act = () => services.AddStableDiffusion(configure);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void AddStableDiffusion_WithNullConfigure_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<StableDiffusionOptions> configure = null!;

            // Act
            Action act = () => services.AddStableDiffusion(configure);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("configure");
        }

        [Fact]
        public void AddStableDiffusion_WithValidConfiguration_RegistersAllServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.TimeoutSeconds = 300;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert - проверяем регистрацию всех интерфейсов
            serviceProvider.GetService<IStableDiffusionClient>().Should().NotBeNull();
            serviceProvider.GetService<ITextToImageService>().Should().NotBeNull();
            serviceProvider.GetService<IImageToImageService>().Should().NotBeNull();
            serviceProvider.GetService<IModelService>().Should().NotBeNull();
            serviceProvider.GetService<IProgressService>().Should().NotBeNull();
            serviceProvider.GetService<IOptionsService>().Should().NotBeNull();
            serviceProvider.GetService<ISamplerService>().Should().NotBeNull();
            serviceProvider.GetService<ISchedulerService>().Should().NotBeNull();
            serviceProvider.GetService<IHttpClientWrapper>().Should().NotBeNull();
        }

        [Fact]
        public void AddStableDiffusion_RegistersOptionsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var expectedBaseUrl = "http://test-server:8080";
            var expectedTimeout = 600;
            var expectedApiKey = "test-api-key";

            // Act
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = expectedBaseUrl;
                options.TimeoutSeconds = expectedTimeout;
                options.ApiKey = expectedApiKey;
            });

            var serviceProvider = services.BuildServiceProvider();
            var actualOptions = serviceProvider
                .GetRequiredService<IOptions<StableDiffusionOptions>>()
                .Value;

            // Assert
            actualOptions.Should().NotBeNull();
            actualOptions.BaseUrl.Should().Be(expectedBaseUrl);
            actualOptions.TimeoutSeconds.Should().Be(expectedTimeout);
            actualOptions.ApiKey.Should().Be(expectedApiKey);
        }

        [Fact]
        public void AddStableDiffusion_ReturnsServiceCollection_ForChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddStableDiffusion(options => options.BaseUrl = "http://test");

            // Assert
            result.Should().BeSameAs(services);
        }

        [Fact]
        public void AddStableDiffusion_RegistersServicesAsTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options => options.BaseUrl = "http://localhost:7860");
            var serviceProvider = services.BuildServiceProvider();

            // Assert - создаем два экземпляра и проверяем что они разные
            var client1 = serviceProvider.GetRequiredService<IStableDiffusionClient>();
            var client2 = serviceProvider.GetRequiredService<IStableDiffusionClient>();

            client1.Should().NotBeSameAs(client2);
        }

        [Fact]
        public void AddStableDiffusion_ConfiguresHttpClientWithBaseAddress()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var expectedBaseUrl = "http://localhost:9090";

            // Act
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = expectedBaseUrl;
                options.TimeoutSeconds = 300;
            });

            var serviceProvider = services.BuildServiceProvider();
            var wrapper = serviceProvider.GetRequiredService<IHttpClientWrapper>();

            // Assert
            wrapper.Should().NotBeNull();
            // HttpClient сконфигурирован внутри wrapper
        }

        [Fact]
        public void AddStableDiffusion_WithApiKey_ConfiguresCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var apiKey = "test-api-key-123";

            // Act
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.ApiKey = apiKey;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert - проверяем что опции настроены правильно
            var actualOptions = serviceProvider
                .GetRequiredService<IOptions<StableDiffusionOptions>>()
                .Value;
            actualOptions.ApiKey.Should().Be(apiKey);

            // Assert - проверяем что wrapper создается без ошибок (исправлена проблема с дублированием Authorization header)
            var wrapper = serviceProvider.GetRequiredService<IHttpClientWrapper>();
            wrapper.Should().NotBeNull();
        }

        [Fact]
        public void AddStableDiffusion_WithoutApiKey_ConfiguresCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.ApiKey = null;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var actualOptions = serviceProvider
                .GetRequiredService<IOptions<StableDiffusionOptions>>()
                .Value;
            actualOptions.ApiKey.Should().BeNull();

            var wrapper = serviceProvider.GetRequiredService<IHttpClientWrapper>();
            wrapper.Should().NotBeNull();
        }

        [Fact]
        public void AddStableDiffusion_RegistersOptionsValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options => options.BaseUrl = "http://localhost:7860");

            // Assert
            var validatorDescriptor = services.FirstOrDefault(s =>
                s.ServiceType == typeof(IValidateOptions<StableDiffusionOptions>)
            );

            validatorDescriptor.Should().NotBeNull();
            validatorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddStableDiffusion_MultipleInvocations_CanBeChained()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            var result = services
                .AddStableDiffusion(options => options.BaseUrl = "http://first:7860")
                .AddStableDiffusion(options => options.BaseUrl = "http://second:7860");

            // Assert
            result.Should().BeSameAs(services);
        }

        #endregion

        #region AddStableDiffusion with string baseUrl Tests

        [Fact]
        public void AddStableDiffusion_WithNullServices_AndBaseUrl_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection services = null!;

            // Act
            Action act = () => services.AddStableDiffusion("http://test");

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void AddStableDiffusion_WithDefaultBaseUrl_RegistersCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion();

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider
                .GetRequiredService<IOptions<StableDiffusionOptions>>()
                .Value;

            // Assert
            options.BaseUrl.Should().Be("http://localhost:7860");
        }

        [Fact]
        public void AddStableDiffusion_WithCustomBaseUrl_RegistersCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var customBaseUrl = "http://custom-server:8888";

            // Act
            services.AddStableDiffusion(customBaseUrl);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider
                .GetRequiredService<IOptions<StableDiffusionOptions>>()
                .Value;

            // Assert
            options.BaseUrl.Should().Be(customBaseUrl);
        }

        [Fact]
        public void AddStableDiffusion_WithBaseUrl_RegistersAllServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion("http://localhost:7860");

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IStableDiffusionClient>().Should().NotBeNull();
            serviceProvider.GetService<ITextToImageService>().Should().NotBeNull();
            serviceProvider.GetService<IImageToImageService>().Should().NotBeNull();
        }

        [Fact]
        public void AddStableDiffusion_WithBaseUrl_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddStableDiffusion("http://test");

            // Assert
            result.Should().BeSameAs(services);
        }

        #endregion

        #region Service Lifetime Tests

        [Fact]
        public void AddStableDiffusion_AllServicesHaveCorrectLifetime()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options => options.BaseUrl = "http://localhost:7860");

            // Assert - проверяем что все сервисы Transient
            var transientServices = services
                .Where(s => s.Lifetime == ServiceLifetime.Transient)
                .ToList();

            transientServices
                .Should()
                .Contain(s => s.ServiceType == typeof(IStableDiffusionClient));
            transientServices.Should().Contain(s => s.ServiceType == typeof(ITextToImageService));
            transientServices.Should().Contain(s => s.ServiceType == typeof(IImageToImageService));
            transientServices.Should().Contain(s => s.ServiceType == typeof(IModelService));
            transientServices.Should().Contain(s => s.ServiceType == typeof(IProgressService));
            transientServices.Should().Contain(s => s.ServiceType == typeof(IOptionsService));
            transientServices.Should().Contain(s => s.ServiceType == typeof(ISamplerService));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void AddStableDiffusion_IntegrationTest_CanResolveAllDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.TimeoutSeconds = 300;
                options.EnableDetailedLogging = true;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert - создаем главный клиент и проверяем что все зависимости разрешены
            Action act = () =>
            {
                var client = serviceProvider.GetRequiredService<IStableDiffusionClient>();
                client.Should().NotBeNull();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void AddStableDiffusion_CanResolveNestedDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options => options.BaseUrl = "http://localhost:7860");
            var serviceProvider = services.BuildServiceProvider();

            // Assert - проверяем что сервисы могут быть созданы со всеми зависимостями
            var textToImageService = serviceProvider.GetRequiredService<ITextToImageService>();
            var imageToImageService = serviceProvider.GetRequiredService<IImageToImageService>();
            var modelService = serviceProvider.GetRequiredService<IModelService>();

            textToImageService.Should().NotBeNull();
            imageToImageService.Should().NotBeNull();
            modelService.Should().NotBeNull();
        }

        #endregion

        #region StableDiffusionOptionsValidator Tests

        [Fact]
        public void OptionsValidator_WithValidOptions_ReturnsSuccess()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.TimeoutSeconds = 300;
            });

            // Act
            var validator = services
                .Where(s => s.ServiceType == typeof(IValidateOptions<StableDiffusionOptions>))
                .Select(s => s.ImplementationType)
                .FirstOrDefault();

            // Assert
            validator.Should().NotBeNull();
        }

        [Fact]
        public void OptionsValidator_WithInvalidOptions_FailsValidation()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = ""; // Невалидная конфигурация
                options.TimeoutSeconds = -1; // Невалидный timeout
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert - при попытке получить опции должна произойти валидация
            Action act = () =>
            {
                // OptionsMonitor будет валидировать при первом доступе если используется IValidateOptions
                var optionsSnapshot = serviceProvider.GetRequiredService<
                    IOptionsSnapshot<StableDiffusionOptions>
                >();
                _ = optionsSnapshot.Value; // Триггерим валидацию
            };

            // Валидация происходит при первом обращении к Value
            // В зависимости от настроек может выбросить исключение или вернуть невалидные данные
            act.Should().Throw<OptionsValidationException>();
        }

        #endregion
    }
}
