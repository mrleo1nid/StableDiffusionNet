using FluentAssertions;
using Newtonsoft.Json;
using StableDiffusionNet.Models.Requests;

namespace StableDiffusionNet.Tests.Models.Requests
{
    /// <summary>
    /// Тесты для ExtraSingleImageRequest
    /// </summary>
    public class ExtraSingleImageRequestTests
    {
        [Fact]
        public void ExtraSingleImageRequest_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var request = new ExtraSingleImageRequest();

            // Assert
            request.Image.Should().Be(string.Empty);
            request.ResizeMode.Should().Be(0);
            request.ShowExtrasResults.Should().BeTrue();
            request.GfpganVisibility.Should().Be(0);
            request.CodeformerVisibility.Should().Be(0);
            request.CodeformerWeight.Should().Be(0.5);
            request.UpscalingResize.Should().Be(2);
            request.UpscalingResizeW.Should().BeNull();
            request.UpscalingResizeH.Should().BeNull();
            request.UpscalingCrop.Should().BeNull();
            request.Upscaler1.Should().BeNull();
            request.Upscaler2.Should().BeNull();
            request.Upscaler2Visibility.Should().Be(0);
            request.UpscaleFirst.Should().BeFalse();
        }

        [Fact]
        public void ExtraSingleImageRequest_SerializesCorrectly()
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "base64_encoded_image_data",
                ResizeMode = 1,
                ShowExtrasResults = false,
                GfpganVisibility = 0.5,
                CodeformerVisibility = 0.7,
                CodeformerWeight = 0.8,
                UpscalingResize = 4,
                UpscalingResizeW = 1024,
                UpscalingResizeH = 1024,
                UpscalingCrop = true,
                Upscaler1 = "ESRGAN_4x",
                Upscaler2 = "Lanczos",
                Upscaler2Visibility = 0.3,
                UpscaleFirst = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtraSingleImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Image.Should().Be("base64_encoded_image_data");
            deserialized.ResizeMode.Should().Be(1);
            deserialized.ShowExtrasResults.Should().BeFalse();
            deserialized.GfpganVisibility.Should().Be(0.5);
            deserialized.CodeformerVisibility.Should().Be(0.7);
            deserialized.CodeformerWeight.Should().Be(0.8);
            deserialized.UpscalingResize.Should().Be(4);
            deserialized.UpscalingResizeW.Should().Be(1024);
            deserialized.UpscalingResizeH.Should().Be(1024);
            deserialized.UpscalingCrop.Should().BeTrue();
            deserialized.Upscaler1.Should().Be("ESRGAN_4x");
            deserialized.Upscaler2.Should().Be("Lanczos");
            deserialized.Upscaler2Visibility.Should().Be(0.3);
            deserialized.UpscaleFirst.Should().BeTrue();
        }

        [Fact]
        public void ExtraSingleImageRequest_WithMinimalSettings_SerializesCorrectly()
        {
            // Arrange
            var request = new ExtraSingleImageRequest { Image = "test_image" };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtraSingleImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Image.Should().Be("test_image");
            deserialized.ResizeMode.Should().Be(0);
            deserialized.UpscalingResize.Should().Be(2);
        }

        [Fact]
        public void ExtraSingleImageRequest_JsonPropertyNames_AreCorrect()
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "test",
                GfpganVisibility = 1.0,
                CodeformerVisibility = 1.0,
                CodeformerWeight = 0.5,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);

            // Assert
            json.Should().Contain("\"image\":");
            json.Should().Contain("\"resize_mode\":");
            json.Should().Contain("\"show_extras_results\":");
            json.Should().Contain("\"gfpgan_visibility\":");
            json.Should().Contain("\"codeformer_visibility\":");
            json.Should().Contain("\"codeformer_weight\":");
            json.Should().Contain("\"upscaling_resize\":");
            json.Should().Contain("\"extras_upscaler_2_visibility\":");
            json.Should().Contain("\"upscale_first\":");
        }

        [Fact]
        public void ExtraSingleImageRequest_WithNullableFields_SerializesCorrectly()
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "test",
                UpscalingResizeW = null,
                UpscalingResizeH = null,
                UpscalingCrop = null,
                Upscaler1 = null,
                Upscaler2 = null,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtraSingleImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.UpscalingResizeW.Should().BeNull();
            deserialized.UpscalingResizeH.Should().BeNull();
            deserialized.UpscalingCrop.Should().BeNull();
            deserialized.Upscaler1.Should().BeNull();
            deserialized.Upscaler2.Should().BeNull();
        }

        [Fact]
        public void ExtraSingleImageRequest_WithUpscalerBlending_SerializesCorrectly()
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "test_image",
                Upscaler1 = "ESRGAN_4x",
                Upscaler2 = "R-ESRGAN 4x+",
                Upscaler2Visibility = 0.5,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtraSingleImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Upscaler1.Should().Be("ESRGAN_4x");
            deserialized.Upscaler2.Should().Be("R-ESRGAN 4x+");
            deserialized.Upscaler2Visibility.Should().Be(0.5);
        }

        [Fact]
        public void ExtraSingleImageRequest_WithCustomDimensions_SerializesCorrectly()
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "test_image",
                ResizeMode = 1,
                UpscalingResizeW = 2048,
                UpscalingResizeH = 1536,
                UpscalingCrop = false,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtraSingleImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.ResizeMode.Should().Be(1);
            deserialized.UpscalingResizeW.Should().Be(2048);
            deserialized.UpscalingResizeH.Should().Be(1536);
            deserialized.UpscalingCrop.Should().BeFalse();
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void ExtraSingleImageRequest_WithDifferentGfpganVisibility_SerializesCorrectly(
            double visibility
        )
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "test",
                GfpganVisibility = visibility,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtraSingleImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.GfpganVisibility.Should().Be(visibility);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void ExtraSingleImageRequest_WithDifferentCodeformerVisibility_SerializesCorrectly(
            double visibility
        )
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "test",
                CodeformerVisibility = visibility,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtraSingleImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.CodeformerVisibility.Should().Be(visibility);
        }
    }
}
