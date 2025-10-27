using FluentAssertions;
using Newtonsoft.Json;
using StableDiffusionNet.Models;
using Xunit;

namespace StableDiffusionNet.Tests.Models
{
    /// <summary>
    /// Тесты для моделей данных API (Sampler, Scheduler, LatentUpscaleMode и др.)
    /// </summary>
    public class ApiModelsTests
    {
        [Fact]
        public void Sampler_SerializesCorrectly()
        {
            // Arrange
            var sampler = new Sampler
            {
                Name = "Euler a",
                Aliases = new System.Collections.Generic.List<string>
                {
                    "euler_ancestral",
                    "euler_a",
                },
                Options = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["uses_ensd"] = true,
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(sampler);
            var deserialized = JsonConvert.DeserializeObject<Sampler>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("Euler a");
            deserialized.Aliases.Should().HaveCount(2);
            deserialized.Options.Should().ContainKey("uses_ensd");
        }

        [Fact]
        public void Scheduler_SerializesCorrectly()
        {
            // Arrange
            var scheduler = new Scheduler
            {
                Name = "karras",
                Label = "Karras",
                Aliases = new System.Collections.Generic.List<string> { "k_karras" },
                DefaultRho = 7.0,
                NeedInnerModel = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(scheduler);
            var deserialized = JsonConvert.DeserializeObject<Scheduler>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("karras");
            deserialized.Label.Should().Be("Karras");
            deserialized.DefaultRho.Should().Be(7.0);
            deserialized.NeedInnerModel.Should().BeTrue();
        }

        [Fact]
        public void LatentUpscaleMode_SerializesCorrectly()
        {
            // Arrange
            var mode = new LatentUpscaleMode { Name = "Bilinear" };

            // Act
            var json = JsonConvert.SerializeObject(mode);
            var deserialized = JsonConvert.DeserializeObject<LatentUpscaleMode>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("Bilinear");
        }

        [Fact]
        public void Embedding_UpdatedFields_SerializeCorrectly()
        {
            // Arrange
            var embedding = new Embedding
            {
                Name = "easy-negative",
                Shape = 768,
                Vectors = 1,
                Step = 1000,
                SdCheckpoint = "abc123",
                SdCheckpointName = "model_v1",
            };

            // Act
            var json = JsonConvert.SerializeObject(embedding);
            var deserialized = JsonConvert.DeserializeObject<Embedding>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Shape.Should().Be(768);
            deserialized.Vectors.Should().Be(1);
            json.Should().Contain("\"shape\":768");
            json.Should().Contain("\"vectors\":1");
        }

        [Fact]
        public void Upscaler_UpdatedFields_SerializeCorrectly()
        {
            // Arrange
            var upscaler = new Upscaler
            {
                Name = "ESRGAN_4x",
                ModelName = "4x-UltraSharp",
                ModelPath = "/models/esrgan/4x-ultrasharp.pth",
                Scale = 4.0,
            };

            // Act
            var json = JsonConvert.SerializeObject(upscaler);
            var deserialized = JsonConvert.DeserializeObject<Upscaler>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.ModelName.Should().Be("4x-UltraSharp");
            json.Should().Contain("\"model_name\":\"4x-UltraSharp\"");
        }

        [Fact]
        public void SdModel_UpdatedFields_SerializeCorrectly()
        {
            // Arrange
            var model = new SdModel
            {
                Title = "Stable Diffusion v1.5",
                ModelName = "v1-5-pruned-emaonly",
                Hash = "cc6cb27103",
                Sha256 = "abc123def456",
                Filename = "/models/checkpoints/v1-5-pruned-emaonly.safetensors",
                Config = "v1-inference.yaml",
            };

            // Act
            var json = JsonConvert.SerializeObject(model);
            var deserialized = JsonConvert.DeserializeObject<SdModel>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Filename.Should().Contain("safetensors");
            json.Should().Contain("\"filename\":");
        }
    }
}
