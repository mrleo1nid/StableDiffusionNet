using FluentAssertions;
using Newtonsoft.Json;
using StableDiffusionNet.Models.Requests;

namespace StableDiffusionNet.Tests.Models.Requests
{
    /// <summary>
    /// Тесты для новых полей в ImageToImageRequest
    /// </summary>
    public class ImageToImageRequestNewFieldsTests
    {
        [Fact]
        public void MaskBlurFields_HaveCorrectDefaultValues()
        {
            // Arrange & Act
            var request = new ImageToImageRequest();

            // Assert
            request.MaskBlurX.Should().Be(4);
            request.MaskBlurY.Should().Be(4);
            request.MaskBlur.Should().BeNull(); // Legacy parameter
            request.MaskRound.Should().BeTrue();
        }

        [Fact]
        public void MaskBlurFields_SerializeCorrectly()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                Prompt = "test",
                InitImages = new System.Collections.Generic.List<string> { "base64_image" },
                MaskBlurX = 8,
                MaskBlurY = 16,
                MaskBlur = 10,
                MaskRound = false,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.MaskBlurX.Should().Be(8);
            deserialized.MaskBlurY.Should().Be(16);
            deserialized.MaskBlur.Should().Be(10);
            deserialized.MaskRound.Should().BeFalse();
        }

        [Fact]
        public void ImageCfgScale_SerializesCorrectly()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                Prompt = "test",
                InitImages = new System.Collections.Generic.List<string> { "base64_image" },
                ImageCfgScale = 1.5,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.ImageCfgScale.Should().Be(1.5);
            json.Should().Contain("\"image_cfg_scale\":1.5");
        }

        [Fact]
        public void InitialNoiseMultiplier_SerializesCorrectly()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                Prompt = "test",
                InitImages = new System.Collections.Generic.List<string> { "base64_image" },
                InitialNoiseMultiplier = 1.2,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.InitialNoiseMultiplier.Should().Be(1.2);
        }

        [Fact]
        public void LatentMask_SerializesCorrectly()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                Prompt = "test",
                InitImages = new System.Collections.Generic.List<string> { "base64_image" },
                LatentMask = "base64_latent_mask",
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.LatentMask.Should().Be("base64_latent_mask");
        }

        [Fact]
        public void IncludeInitImages_SerializesCorrectly()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                Prompt = "test",
                InitImages = new System.Collections.Generic.List<string> { "base64_image" },
                IncludeInitImages = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.IncludeInitImages.Should().BeTrue();
        }

        [Fact]
        public void AllNewFields_SerializeCorrectly()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                Prompt = "test",
                InitImages = new System.Collections.Generic.List<string> { "base64_image" },
                Styles = new System.Collections.Generic.List<string> { "style1" },
                Subseed = 999,
                SubseedStrength = 0.3,
                DistilledCfgScale = 3.5,
                Eta = 0.6,
                DoNotSaveSamples = true,
                ImageCfgScale = 1.5,
                MaskBlurX = 10,
                MaskBlurY = 12,
                MaskRound = false,
                InitialNoiseMultiplier = 1.1,
                RefinerCheckpoint = "refiner.safetensors",
                RefinerSwitchAt = 0.75,
                DisableExtraNetworks = true,
                ScriptName = "test_script",
                IncludeInitImages = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<ImageToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Styles.Should().HaveCount(1);
            deserialized.Subseed.Should().Be(999);
            deserialized.SubseedStrength.Should().Be(0.3);
            deserialized.DistilledCfgScale.Should().Be(3.5);
            deserialized.Eta.Should().Be(0.6);
            deserialized.DoNotSaveSamples.Should().BeTrue();
            deserialized.ImageCfgScale.Should().Be(1.5);
            deserialized.MaskBlurX.Should().Be(10);
            deserialized.MaskBlurY.Should().Be(12);
            deserialized.MaskRound.Should().BeFalse();
            deserialized.InitialNoiseMultiplier.Should().Be(1.1);
            deserialized.RefinerCheckpoint.Should().Be("refiner.safetensors");
            deserialized.RefinerSwitchAt.Should().Be(0.75);
            deserialized.DisableExtraNetworks.Should().BeTrue();
            deserialized.ScriptName.Should().Be("test_script");
            deserialized.IncludeInitImages.Should().BeTrue();
        }
    }
}
