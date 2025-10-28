using FluentAssertions;
using StableDiffusionNet.Tests;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для базовых API операций
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public sealed class BasicApiIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetSamplersAsync_ReturnsListOfSamplers()
        {
            // Arrange
            var isApiAvailable = await IsApiAvailableAsync();
            if (!isApiAvailable)
            {
                // Пропускаем тест если API недоступен
                return;
            }

            // Act
            var samplers = await Client.Samplers.GetSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();
            samplers.Should().NotBeEmpty();
            samplers
                .Should()
                .Contain(s => s.Name.Contains("Euler", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetSchedulersAsync_ReturnsListOfSchedulers()
        {
            // Act
            var schedulers = await Client.Schedulers.GetSchedulersAsync();

            // Assert
            schedulers.Should().NotBeNull();
            schedulers.Should().NotBeEmpty();
            schedulers
                .Should()
                .Contain(s =>
                    s.Name.Contains("automatic", StringComparison.OrdinalIgnoreCase)
                    || s.Name.Contains("karras", StringComparison.OrdinalIgnoreCase)
                    || s.Name.Contains("uniform", StringComparison.OrdinalIgnoreCase)
                );
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetModelsAsync_ReturnsListOfModels()
        {
            // Act
            var models = await Client.Models.GetModelsAsync();

            // Assert
            models.Should().NotBeNull();
            models.Should().NotBeEmpty();
            models[0].Title.Should().NotBeNullOrEmpty();
            models[0].ModelName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetCurrentModelAsync_ReturnsModelName()
        {
            // Act
            var currentModel = await Client.Models.GetCurrentModelAsync();

            // Assert
            currentModel.Should().NotBeNullOrEmpty();
            currentModel.Should().NotBe("unknown");
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetOptionsAsync_ReturnsWebUIOptions()
        {
            // Act
            var options = await Client.Options.GetOptionsAsync();

            // Assert
            options.Should().NotBeNull();
            options.SdModelCheckpoint.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetProgressAsync_ReturnsProgress()
        {
            // Act
            var progress = await Client.Progress.GetProgressAsync();

            // Assert
            progress.Should().NotBeNull();
            progress.Progress.Should().BeInRange(0.0, 1.0);
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task HealthCheckAsync_WithRunningAPI_ReturnsHealthy()
        {
            // Act
            var healthCheck = await Client.HealthCheckAsync();

            // Assert
            healthCheck.Should().NotBeNull();
            healthCheck.IsHealthy.Should().BeTrue();
            healthCheck.ResponseTime.Should().NotBeNull();
            healthCheck.ResponseTime.Should().BeLessThan(TimeSpan.FromSeconds(5));
            healthCheck.Error.Should().BeNull();
        }
    }
}
