using FluentAssertions;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Tests.Models.Responses
{
    /// <summary>
    /// Тесты для модели ImageToImageResponse
    /// </summary>
    public class ImageToImageResponseTests
    {
        [Fact]
        public void Constructor_DefaultValues_InitializesCorrectly()
        {
            // Act
            var response = new ImageToImageResponse();

            // Assert
            response.Images.Should().BeNull();
            response.Parameters.Should().BeNull();
            response.Info.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithValues_InitializesCorrectly()
        {
            // Arrange
            var images = new List<string> { "image1", "image2" };
            var parameters = new Dictionary<string, object>
            {
                { "steps", 20 },
                { "cfg_scale", 7.5 },
            };
            var info = "Generation info";

            // Act
            var response = new ImageToImageResponse
            {
                Images = images,
                Parameters = parameters,
                Info = info,
            };

            // Assert
            response.Images.Should().BeEquivalentTo(images);
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Info.Should().Be(info);
        }

        [Fact]
        public void Constructor_WithEmptyImages_InitializesCorrectly()
        {
            // Arrange
            var emptyImages = new List<string>();
            var parameters = new Dictionary<string, object> { { "steps", 10 } };
            var info = "No images generated";

            // Act
            var response = new ImageToImageResponse
            {
                Images = emptyImages,
                Parameters = parameters,
                Info = info,
            };

            // Assert
            response.Images.Should().BeEmpty();
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Info.Should().Be(info);
        }

        [Fact]
        public void Constructor_WithNullValues_InitializesCorrectly()
        {
            // Act
            var response = new ImageToImageResponse
            {
                Images = null,
                Parameters = null,
                Info = null,
            };

            // Assert
            response.Images.Should().BeNull();
            response.Parameters.Should().BeNull();
            response.Info.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithComplexParameters_InitializesCorrectly()
        {
            // Arrange
            var images = new List<string> { "image1" };
            var complexParameters = new Dictionary<string, object>
            {
                { "steps", 25 },
                { "cfg_scale", 8.0 },
                { "sampler_name", "Euler a" },
                { "width", 512 },
                { "height", 512 },
                { "denoising_strength", 0.75 },
                { "enable_hr", true },
                { "hr_scale", 2.0 },
            };
            var info = "Complex generation completed";

            // Act
            var response = new ImageToImageResponse
            {
                Images = images,
                Parameters = complexParameters,
                Info = info,
            };

            // Assert
            response.Images.Should().BeEquivalentTo(images);
            response.Parameters.Should().BeEquivalentTo(complexParameters);
            response.Info.Should().Be(info);
        }

        [Fact]
        public void Record_Equality_WorksCorrectly()
        {
            // Arrange
            var images = new List<string> { "image1", "image2" };
            var parameters = new Dictionary<string, object> { { "steps", 20 } };
            var info = "Generation info";

            var response1 = new ImageToImageResponse
            {
                Images = images,
                Parameters = parameters,
                Info = info,
            };

            var response2 = new ImageToImageResponse
            {
                Images = images,
                Parameters = parameters,
                Info = info,
            };

            // Act & Assert
            response1.Should().Be(response2);
            response1.GetHashCode().Should().Be(response2.GetHashCode());
        }

        [Fact]
        public void Record_Inequality_WorksCorrectly()
        {
            // Arrange
            var response1 = new ImageToImageResponse
            {
                Images = new List<string> { "image1" },
                Parameters = new Dictionary<string, object> { { "steps", 10 } },
                Info = "Info 1",
            };

            var response2 = new ImageToImageResponse
            {
                Images = new List<string> { "image2" },
                Parameters = new Dictionary<string, object> { { "steps", 20 } },
                Info = "Info 2",
            };

            // Act & Assert
            response1.Should().NotBe(response2);
            response1.GetHashCode().Should().NotBe(response2.GetHashCode());
        }

        [Fact]
        public void Record_ToString_ContainsRelevantInformation()
        {
            // Arrange
            var images = new List<string> { "image1", "image2" };
            var parameters = new Dictionary<string, object> { { "steps", 20 } };
            var info = "Generation info";

            var response = new ImageToImageResponse
            {
                Images = images,
                Parameters = parameters,
                Info = info,
            };

            // Act
            var toString = response.ToString();

            // Assert
            toString.Should().Contain("Images = System.Collections.Generic.List`1[System.String]");
            toString
                .Should()
                .Contain(
                    "Parameters = System.Collections.Generic.Dictionary`2[System.String,System.Object]"
                );
            toString.Should().Contain("Info = Generation info");
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(new[] { "image1" }, "Info")]
        [InlineData(new[] { "image1", "image2" }, "Complete info")]
        public void Constructor_WithVariousValues_InitializesCorrectly(
            string[]? imagesArray,
            string? info
        )
        {
            // Arrange
            var images = imagesArray?.ToList();
            var parameters =
                imagesArray != null ? new Dictionary<string, object> { { "steps", 10 } } : null;

            // Act
            var response = new ImageToImageResponse
            {
                Images = images,
                Parameters = parameters,
                Info = info,
            };

            // Assert
            response.Images.Should().BeEquivalentTo(images);
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Info.Should().Be(info);
        }

        [Fact]
        public void Constructor_WithSingleImage_InitializesCorrectly()
        {
            // Arrange
            var singleImage = new List<string> { "single_image_base64" };
            var parameters = new Dictionary<string, object> { { "steps", 15 } };
            var info = "Single image generated";

            // Act
            var response = new ImageToImageResponse
            {
                Images = singleImage,
                Parameters = parameters,
                Info = info,
            };

            // Assert
            response.Images.Should().HaveCount(1);
            response.Images.Should().Contain("single_image_base64");
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Info.Should().Be(info);
        }

        [Fact]
        public void Constructor_WithMultipleImages_InitializesCorrectly()
        {
            // Arrange
            var multipleImages = new List<string>
            {
                "image1_base64",
                "image2_base64",
                "image3_base64",
            };
            var parameters = new Dictionary<string, object> { { "batch_size", 3 } };
            var info = "Multiple images generated";

            // Act
            var response = new ImageToImageResponse
            {
                Images = multipleImages,
                Parameters = parameters,
                Info = info,
            };

            // Assert
            response.Images.Should().HaveCount(3);
            response.Images.Should().Contain("image1_base64");
            response.Images.Should().Contain("image2_base64");
            response.Images.Should().Contain("image3_base64");
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Info.Should().Be(info);
        }

        [Fact]
        public void Constructor_WithLongInfo_InitializesCorrectly()
        {
            // Arrange
            var images = new List<string> { "image1" };
            var parameters = new Dictionary<string, object> { { "steps", 10 } };
            var longInfo = new string('A', 1000); // Длинная строка с информацией

            // Act
            var response = new ImageToImageResponse
            {
                Images = images,
                Parameters = parameters,
                Info = longInfo,
            };

            // Assert
            response.Images.Should().BeEquivalentTo(images);
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Info.Should().Be(longInfo);
        }
    }
}
