using FluentAssertions;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Tests.Models.Responses
{
    /// <summary>
    /// Тесты для модели ExtraSingleImageResponse
    /// </summary>
    public class ExtraSingleImageResponseTests
    {
        [Fact]
        public void Constructor_DefaultValues_InitializesCorrectly()
        {
            // Act
            var response = new ExtraSingleImageResponse();

            // Assert
            response.Image.Should().BeNull();
            response.HtmlInfo.Should().Be(string.Empty);
        }

        [Fact]
        public void Constructor_WithValues_InitializesCorrectly()
        {
            // Arrange
            var image =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var htmlInfo = "<p>Test HTML info</p>";

            // Act
            var response = new ExtraSingleImageResponse { Image = image, HtmlInfo = htmlInfo };

            // Assert
            response.Image.Should().Be(image);
            response.HtmlInfo.Should().Be(htmlInfo);
        }

        [Fact]
        public void Constructor_WithNullImage_InitializesCorrectly()
        {
            // Arrange
            var htmlInfo = "<p>Test HTML info</p>";

            // Act
            var response = new ExtraSingleImageResponse { Image = null, HtmlInfo = htmlInfo };

            // Assert
            response.Image.Should().BeNull();
            response.HtmlInfo.Should().Be(htmlInfo);
        }

        [Fact]
        public void Constructor_WithEmptyHtmlInfo_InitializesCorrectly()
        {
            // Arrange
            var image =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            // Act
            var response = new ExtraSingleImageResponse { Image = image, HtmlInfo = string.Empty };

            // Assert
            response.Image.Should().Be(image);
            response.HtmlInfo.Should().Be(string.Empty);
        }

        [Fact]
        public void Record_Equality_WorksCorrectly()
        {
            // Arrange
            var image =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var htmlInfo = "<p>Test HTML info</p>";

            var response1 = new ExtraSingleImageResponse { Image = image, HtmlInfo = htmlInfo };

            var response2 = new ExtraSingleImageResponse { Image = image, HtmlInfo = htmlInfo };

            // Act & Assert
            response1.Should().Be(response2);
            response1.GetHashCode().Should().Be(response2.GetHashCode());
        }

        [Fact]
        public void Record_Inequality_WorksCorrectly()
        {
            // Arrange
            var response1 = new ExtraSingleImageResponse
            {
                Image = "image1",
                HtmlInfo = "<p>Info 1</p>",
            };

            var response2 = new ExtraSingleImageResponse
            {
                Image = "image2",
                HtmlInfo = "<p>Info 2</p>",
            };

            // Act & Assert
            response1.Should().NotBe(response2);
            response1.GetHashCode().Should().NotBe(response2.GetHashCode());
        }

        [Fact]
        public void Record_ToString_ContainsRelevantInformation()
        {
            // Arrange
            var image =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var htmlInfo = "<p>Test HTML info</p>";

            var response = new ExtraSingleImageResponse { Image = image, HtmlInfo = htmlInfo };

            // Act
            var toString = response.ToString();

            // Assert
            toString
                .Should()
                .Contain(
                    "Image = iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
                );
            toString.Should().Contain("HtmlInfo = <p>Test HTML info</p>");
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "<p>Test</p>")]
        [InlineData(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            ""
        )]
        [InlineData(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            "<p>Test HTML info</p>"
        )]
        public void Constructor_WithVariousValues_InitializesCorrectly(
            string? image,
            string htmlInfo
        )
        {
            // Act
            var response = new ExtraSingleImageResponse { Image = image, HtmlInfo = htmlInfo };

            // Assert
            response.Image.Should().Be(image);
            response.HtmlInfo.Should().Be(htmlInfo);
        }

        [Fact]
        public void Constructor_WithLongImageData_InitializesCorrectly()
        {
            // Arrange
            var longImage = new string('A', 10000); // Длинная строка base64
            var htmlInfo = "<p>Test HTML info</p>";

            // Act
            var response = new ExtraSingleImageResponse { Image = longImage, HtmlInfo = htmlInfo };

            // Assert
            response.Image.Should().Be(longImage);
            response.HtmlInfo.Should().Be(htmlInfo);
        }

        [Fact]
        public void Constructor_WithComplexHtmlInfo_InitializesCorrectly()
        {
            // Arrange
            var image =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var complexHtmlInfo =
                @"
                <div class='result'>
                    <h3>Processing Results</h3>
                    <p>Image processed successfully</p>
                    <ul>
                        <li>Upscaling: 2x</li>
                        <li>Format: PNG</li>
                    </ul>
                </div>";

            // Act
            var response = new ExtraSingleImageResponse
            {
                Image = image,
                HtmlInfo = complexHtmlInfo,
            };

            // Assert
            response.Image.Should().Be(image);
            response.HtmlInfo.Should().Be(complexHtmlInfo);
        }
    }
}
