using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Tests.Models.Responses
{
    /// <summary>
    /// Тесты для ImageToImageResponse
    /// </summary>
    public class ImageToImageResponseTests
    {
        [Fact]
        public void ImageToImageResponse_SerializesCorrectly()
        {
            // Arrange
            var response = new ImageToImageResponse
            {
                Images = new List<string> { "base64_image_1", "base64_image_2" },
                Parameters = new Dictionary<string, object>
                {
                    ["prompt"] = "test prompt",
                    ["steps"] = 20,
                },
                Info = "Generation completed successfully",
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageResponse>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Images.Should().NotBeNull();
            deserialized.Images.Should().HaveCount(2);
            deserialized.Images![0].Should().Be("base64_image_1");
            deserialized.Images[1].Should().Be("base64_image_2");
            deserialized.Parameters.Should().NotBeNull();
            deserialized.Parameters.Should().ContainKey("prompt");
            deserialized.Info.Should().Be("Generation completed successfully");
        }

        [Fact]
        public void ImageToImageResponse_DeserializesCorrectly()
        {
            // Arrange
            var json =
                "{\"images\":[\"img1\",\"img2\"],\"parameters\":{\"seed\":123},\"info\":\"test info\"}";

            // Act
            var response = JsonConvert.DeserializeObject<ImageToImageResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Images.Should().HaveCount(2);
            response.Parameters.Should().ContainKey("seed");
            response.Info.Should().Be("test info");
        }

        [Fact]
        public void ImageToImageResponse_WithNullValues_SerializesCorrectly()
        {
            // Arrange
            var response = new ImageToImageResponse
            {
                Images = null,
                Parameters = null,
                Info = null,
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageResponse>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Images.Should().BeNull();
            deserialized.Parameters.Should().BeNull();
            deserialized.Info.Should().BeNull();
        }

        [Fact]
        public void ImageToImageResponse_WithEmptyLists_SerializesCorrectly()
        {
            // Arrange
            var response = new ImageToImageResponse
            {
                Images = new List<string>(),
                Parameters = new Dictionary<string, object>(),
                Info = string.Empty,
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageResponse>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Images.Should().NotBeNull();
            deserialized.Images.Should().BeEmpty();
            deserialized.Parameters.Should().NotBeNull();
            deserialized.Parameters.Should().BeEmpty();
            deserialized.Info.Should().Be(string.Empty);
        }

        [Fact]
        public void ImageToImageResponse_JsonPropertyNames_AreCorrect()
        {
            // Arrange
            var response = new ImageToImageResponse
            {
                Images = new List<string> { "test" },
                Parameters = new Dictionary<string, object> { ["key"] = "value" },
                Info = "info",
            };

            // Act
            var json = JsonConvert.SerializeObject(response);

            // Assert
            json.Should().Contain("\"images\":");
            json.Should().Contain("\"parameters\":");
            json.Should().Contain("\"info\":");
        }

        [Fact]
        public void ImageToImageResponse_WithComplexParameters_SerializesCorrectly()
        {
            // Arrange
            var response = new ImageToImageResponse
            {
                Images = new List<string> { "image_data" },
                Parameters = new Dictionary<string, object>
                {
                    ["prompt"] = "beautiful landscape",
                    ["negative_prompt"] = "ugly",
                    ["steps"] = 30,
                    ["cfg_scale"] = 7.5,
                    ["seed"] = 123456789,
                    ["width"] = 512,
                    ["height"] = 512,
                },
                Info = "{\"seed\":123456789}",
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageResponse>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Parameters.Should().HaveCount(7);
            deserialized.Parameters.Should().ContainKey("prompt");
            deserialized.Parameters.Should().ContainKey("seed");
        }

        [Fact]
        public void ImageToImageResponse_WithSingleImage_SerializesCorrectly()
        {
            // Arrange
            var response = new ImageToImageResponse
            {
                Images = new List<string> { "single_image_base64" },
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageResponse>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Images.Should().HaveCount(1);
            deserialized.Images![0].Should().Be("single_image_base64");
        }

        [Fact]
        public void ImageToImageResponse_WithMultipleImages_SerializesCorrectly()
        {
            // Arrange
            var response = new ImageToImageResponse
            {
                Images = new List<string> { "image1", "image2", "image3", "image4" },
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageResponse>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Images.Should().HaveCount(4);
        }
    }
}
