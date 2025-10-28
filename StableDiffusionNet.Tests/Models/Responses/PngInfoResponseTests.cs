using FluentAssertions;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Tests.Models.Responses
{
    /// <summary>
    /// Тесты для модели PngInfoResponse
    /// </summary>
    public class PngInfoResponseTests
    {
        [Fact]
        public void Constructor_DefaultValues_InitializesCorrectly()
        {
            // Act
            var response = new PngInfoResponse();

            // Assert
            response.Info.Should().Be(string.Empty);
            response.Items.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithValues_InitializesCorrectly()
        {
            // Arrange
            var info = "Steps: 20, Sampler: Euler, CFG scale: 7.0";
            var items = new { metadata = "test" };

            // Act
            var response = new PngInfoResponse { Info = info, Items = items };

            // Assert
            response.Info.Should().Be(info);
            response.Items.Should().Be(items);
        }

        [Fact]
        public void Constructor_WithEmptyInfo_InitializesCorrectly()
        {
            // Arrange
            var items = new { metadata = "test" };

            // Act
            var response = new PngInfoResponse { Info = string.Empty, Items = items };

            // Assert
            response.Info.Should().Be(string.Empty);
            response.Items.Should().Be(items);
        }

        [Fact]
        public void Constructor_WithNullItems_InitializesCorrectly()
        {
            // Arrange
            var info = "Steps: 20, Sampler: Euler, CFG scale: 7.0";

            // Act
            var response = new PngInfoResponse { Info = info, Items = null };

            // Assert
            response.Info.Should().Be(info);
            response.Items.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithComplexInfo_InitializesCorrectly()
        {
            // Arrange
            var complexInfo =
                @"
Steps: 20, Sampler: Euler, Schedule type: Automatic, CFG scale: 7.0, 
Seed: 1234567890, Size: 512x512, Model hash: abc123def456, 
Model: test_model.safetensors, Clip skip: 2, RNG: CPU, Version: neo";
            var complexItems = new
            {
                parameters = new
                {
                    Steps = "20",
                    Sampler = "Euler",
                    CFG_scale = "7.0",
                    Seed = "1234567890",
                },
                metadata = new
                {
                    model_hash = "abc123def456",
                    model_name = "test_model.safetensors",
                },
            };

            // Act
            var response = new PngInfoResponse { Info = complexInfo, Items = complexItems };

            // Assert
            response.Info.Should().Be(complexInfo);
            response.Items.Should().Be(complexItems);
        }

        [Fact]
        public void Constructor_WithDictionaryItems_InitializesCorrectly()
        {
            // Arrange
            var info = "Steps: 20, Sampler: Euler";
            var dictionaryItems = new Dictionary<string, object>
            {
                { "Steps", "20" },
                { "Sampler", "Euler" },
                { "CFG_scale", 7.0 },
                { "Seed", 1234567890L },
            };

            // Act
            var response = new PngInfoResponse { Info = info, Items = dictionaryItems };

            // Assert
            response.Info.Should().Be(info);
            response.Items.Should().Be(dictionaryItems);
        }

        [Fact]
        public void Record_Equality_WorksCorrectly()
        {
            // Arrange
            var info = "Steps: 20, Sampler: Euler, CFG scale: 7.0";
            var items = new { metadata = "test" };

            var response1 = new PngInfoResponse { Info = info, Items = items };

            var response2 = new PngInfoResponse { Info = info, Items = items };

            // Act & Assert
            response1.Should().Be(response2);
            response1.GetHashCode().Should().Be(response2.GetHashCode());
        }

        [Fact]
        public void Record_Inequality_WorksCorrectly()
        {
            // Arrange
            var response1 = new PngInfoResponse
            {
                Info = "Info 1",
                Items = new { metadata = "test1" },
            };

            var response2 = new PngInfoResponse
            {
                Info = "Info 2",
                Items = new { metadata = "test2" },
            };

            // Act & Assert
            response1.Should().NotBe(response2);
            response1.GetHashCode().Should().NotBe(response2.GetHashCode());
        }

        [Fact]
        public void Record_ToString_ContainsRelevantInformation()
        {
            // Arrange
            var info = "Steps: 20, Sampler: Euler, CFG scale: 7.0";
            var items = new { metadata = "test" };

            var response = new PngInfoResponse { Info = info, Items = items };

            // Act
            var toString = response.ToString();

            // Assert
            toString.Should().Contain("Info = Steps: 20, Sampler: Euler, CFG scale: 7.0");
            toString.Should().Contain("Items = { metadata = test }");
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("Steps: 20", null)]
        [InlineData("", "test")]
        [InlineData("Steps: 20, Sampler: Euler", "test")]
        public void Constructor_WithVariousValues_InitializesCorrectly(
            string info,
            string? itemsString
        )
        {
            // Arrange
            object? items = itemsString != null ? new { metadata = itemsString } : null;

            // Act
            var response = new PngInfoResponse { Info = info, Items = items };

            // Assert
            response.Info.Should().Be(info);
            response.Items.Should().Be(items);
        }

        [Fact]
        public void Constructor_WithLongInfo_InitializesCorrectly()
        {
            // Arrange
            var longInfo = new string('A', 1000); // Длинная строка с информацией
            var items = new { metadata = "test" };

            // Act
            var response = new PngInfoResponse { Info = longInfo, Items = items };

            // Assert
            response.Info.Should().Be(longInfo);
            response.Items.Should().Be(items);
        }

        [Fact]
        public void Constructor_WithEmptyStringInfo_InitializesCorrectly()
        {
            // Arrange
            var items = new { metadata = "test" };

            // Act
            var response = new PngInfoResponse { Info = "", Items = items };

            // Assert
            response.Info.Should().Be("");
            response.Items.Should().Be(items);
        }

        [Fact]
        public void Constructor_WithNullItemsAndEmptyInfo_InitializesCorrectly()
        {
            // Act
            var response = new PngInfoResponse { Info = "", Items = null };

            // Assert
            response.Info.Should().Be("");
            response.Items.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithJsonStringItems_InitializesCorrectly()
        {
            // Arrange
            var info = "Steps: 20, Sampler: Euler";
            var jsonStringItems = "{\"Steps\":\"20\",\"Sampler\":\"Euler\",\"CFG_scale\":7.0}";

            // Act
            var response = new PngInfoResponse { Info = info, Items = jsonStringItems };

            // Assert
            response.Info.Should().Be(info);
            response.Items.Should().Be(jsonStringItems);
        }
    }
}
