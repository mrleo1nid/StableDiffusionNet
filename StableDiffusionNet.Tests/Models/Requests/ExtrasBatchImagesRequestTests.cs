using FluentAssertions;
using Newtonsoft.Json;
using StableDiffusionNet.Models.Requests;
using Xunit;

namespace StableDiffusionNet.Tests.Models.Requests
{
    /// <summary>
    /// Тесты для ExtrasBatchImagesRequest
    /// </summary>
    public class ExtrasBatchImagesRequestTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Arrange & Act
            var request = new ExtrasBatchImagesRequest();

            // Assert
            request.ImageList.Should().NotBeNull();
            request.ImageList.Should().BeEmpty();
            request.ResizeMode.Should().Be(0);
            request.ShowExtrasResults.Should().BeTrue();
            request.GfpganVisibility.Should().Be(0);
            request.CodeformerVisibility.Should().Be(0);
            request.CodeformerWeight.Should().Be(0);
            request.UpscalingResize.Should().Be(2);
            request.UpscalingResizeW.Should().Be(512);
            request.UpscalingResizeH.Should().Be(512);
            request.UpscalingCrop.Should().BeTrue();
            request.Upscaler1.Should().Be("None");
            request.Upscaler2.Should().Be("None");
            request.Upscaler2Visibility.Should().Be(0);
            request.UpscaleFirst.Should().BeFalse();
        }

        [Fact]
        public void SerializesCorrectly()
        {
            // Arrange
            var request = new ExtrasBatchImagesRequest
            {
                ImageList = new System.Collections.Generic.List<FileData>
                {
                    new FileData { Data = "base64_data1", Name = "image1.png" },
                    new FileData { Data = "base64_data2", Name = "image2.png" },
                },
                ResizeMode = 1,
                UpscalingResize = 4,
                Upscaler1 = "ESRGAN_4x",
                UpscaleFirst = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtrasBatchImagesRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.ImageList.Should().HaveCount(2);
            deserialized.ImageList[0].Name.Should().Be("image1.png");
            deserialized.ImageList[1].Name.Should().Be("image2.png");
            deserialized.ResizeMode.Should().Be(1);
            deserialized.UpscalingResize.Should().Be(4);
            deserialized.Upscaler1.Should().Be("ESRGAN_4x");
            deserialized.UpscaleFirst.Should().BeTrue();
        }

        [Fact]
        public void FileData_SerializesCorrectly()
        {
            // Arrange
            var fileData = new FileData { Data = "base64_encoded_data", Name = "test_image.png" };

            // Act
            var json = JsonConvert.SerializeObject(fileData);
            var deserialized = JsonConvert.DeserializeObject<FileData>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Data.Should().Be("base64_encoded_data");
            deserialized.Name.Should().Be("test_image.png");
        }

        [Fact]
        public void FaceRestorationParameters_SerializeCorrectly()
        {
            // Arrange
            var request = new ExtrasBatchImagesRequest
            {
                ImageList = new System.Collections.Generic.List<FileData>
                {
                    new FileData { Data = "base64_data", Name = "image.png" },
                },
                GfpganVisibility = 0.8,
                CodeformerVisibility = 0.6,
                CodeformerWeight = 0.7,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ExtrasBatchImagesRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.GfpganVisibility.Should().Be(0.8);
            deserialized.CodeformerVisibility.Should().Be(0.6);
            deserialized.CodeformerWeight.Should().Be(0.7);
        }

        [Fact]
        public void JsonPropertyNames_AreCorrect()
        {
            // Arrange
            var request = new ExtrasBatchImagesRequest
            {
                ImageList = new System.Collections.Generic.List<FileData>
                {
                    new FileData { Data = "test", Name = "test.png" },
                },
                UpscaleFirst = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);

            // Assert
            json.Should().Contain("\"imageList\":");
            json.Should().Contain("\"upscale_first\":true");
            json.Should().Contain("\"gfpgan_visibility\":");
            json.Should().Contain("\"codeformer_visibility\":");
        }
    }
}
