using FluentAssertions;
using Newtonsoft.Json;
using StableDiffusionNet.Models.Responses;
using Xunit;

namespace StableDiffusionNet.Tests.Models.Responses
{
    /// <summary>
    /// Тесты для ExtrasBatchImagesResponse
    /// </summary>
    public class ExtrasBatchImagesResponseTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Arrange & Act
            var response = new ExtrasBatchImagesResponse();

            // Assert
            response.HtmlInfo.Should().Be(string.Empty);
            response.Images.Should().NotBeNull();
            response.Images.Should().BeEmpty();
        }

        [Fact]
        public void SerializesCorrectly()
        {
            // Arrange
            var response = new ExtrasBatchImagesResponse
            {
                HtmlInfo = "<p>Processed 2 images</p>",
                Images = new System.Collections.Generic.List<string>
                {
                    "base64_image_data_1",
                    "base64_image_data_2",
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<ExtrasBatchImagesResponse>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.HtmlInfo.Should().Be("<p>Processed 2 images</p>");
            deserialized.Images.Should().HaveCount(2);
            deserialized.Images[0].Should().Be("base64_image_data_1");
            deserialized.Images[1].Should().Be("base64_image_data_2");
        }

        [Fact]
        public void DeserializesFromJson()
        {
            // Arrange
            var json =
                @"{
                ""html_info"": ""<p>Processing complete</p>"",
                ""images"": [""img1_base64"", ""img2_base64"", ""img3_base64""]
            }";

            // Act
            var response = JsonConvert.DeserializeObject<ExtrasBatchImagesResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.HtmlInfo.Should().Be("<p>Processing complete</p>");
            response.Images.Should().HaveCount(3);
            response.Images.Should().Contain("img1_base64");
            response.Images.Should().Contain("img2_base64");
            response.Images.Should().Contain("img3_base64");
        }

        [Fact]
        public void JsonPropertyNames_AreCorrect()
        {
            // Arrange
            var response = new ExtrasBatchImagesResponse
            {
                HtmlInfo = "test info",
                Images = new System.Collections.Generic.List<string> { "test_image" },
            };

            // Act
            var json = JsonConvert.SerializeObject(response);

            // Assert
            json.Should().Contain("\"html_info\":");
            json.Should().Contain("\"images\":");
            json.Should().NotContain("\"HtmlInfo\":");
            json.Should().NotContain("\"Images\":");
        }

        [Fact]
        public void HandlesEmptyResponse()
        {
            // Arrange
            var json = @"{""html_info"": """", ""images"": []}";

            // Act
            var response = JsonConvert.DeserializeObject<ExtrasBatchImagesResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.HtmlInfo.Should().Be(string.Empty);
            response.Images.Should().BeEmpty();
        }

        [Fact]
        public void HandlesMissingFields_UsesDefaultValues()
        {
            // Arrange
            var json = @"{}";

            // Act
            var response = JsonConvert.DeserializeObject<ExtrasBatchImagesResponse>(json);

            // Assert
            response.Should().NotBeNull();
            // Свойства имеют дефолтные значения, поэтому не будут null
            response!.HtmlInfo.Should().NotBeNull();
            response.Images.Should().NotBeNull();
        }

        [Fact]
        public void PropertiesCanBeSetAndRetrieved()
        {
            // Arrange
            var response = new ExtrasBatchImagesResponse();
            var testHtmlInfo = "<p>Test HTML</p>";
            var testImages = new System.Collections.Generic.List<string> { "image1", "image2" };

            // Act
            response.HtmlInfo = testHtmlInfo;
            response.Images = testImages;

            // Assert
            response.HtmlInfo.Should().Be(testHtmlInfo);
            response.Images.Should().BeSameAs(testImages);
            response.Images.Should().HaveCount(2);
        }
    }
}
