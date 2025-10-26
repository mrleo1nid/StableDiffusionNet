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
        public void FaceRestorer_SerializesCorrectly()
        {
            // Arrange
            var restorer = new FaceRestorer { Name = "CodeFormer", CmdDir = "/models/codeformer" };

            // Act
            var json = JsonConvert.SerializeObject(restorer);
            var deserialized = JsonConvert.DeserializeObject<FaceRestorer>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("CodeFormer");
            deserialized.CmdDir.Should().Be("/models/codeformer");
        }

        [Fact]
        public void PromptStyle_SerializesCorrectly()
        {
            // Arrange
            var style = new PromptStyle
            {
                Name = "Cinematic",
                Prompt = "cinematic lighting, professional photography",
                NegativePrompt = "low quality, amateur",
            };

            // Act
            var json = JsonConvert.SerializeObject(style);
            var deserialized = JsonConvert.DeserializeObject<PromptStyle>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("Cinematic");
            deserialized.Prompt.Should().Be("cinematic lighting, professional photography");
            deserialized.NegativePrompt.Should().Be("low quality, amateur");
        }

        [Fact]
        public void MemoryInfo_SerializesCorrectly()
        {
            // Arrange
            var memoryInfo = new MemoryInfo
            {
                Ram = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["free"] = 8192,
                    ["used"] = 16384,
                    ["total"] = 32768,
                },
                Cuda = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["free"] = 4096,
                    ["used"] = 8192,
                    ["total"] = 12288,
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(memoryInfo);
            var deserialized = JsonConvert.DeserializeObject<MemoryInfo>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Ram.Should().ContainKey("free");
            deserialized.Cuda.Should().ContainKey("total");
        }

        [Fact]
        public void ScriptInfo_SerializesCorrectly()
        {
            // Arrange
            var scriptInfo = new ScriptInfo
            {
                Name = "XYZ Grid",
                IsAlwayson = false,
                IsImg2img = false,
                Args = new System.Collections.Generic.List<ScriptArg>
                {
                    new ScriptArg
                    {
                        Label = "X axis",
                        Value = "Steps",
                        Choices = new System.Collections.Generic.List<string>
                        {
                            "Steps",
                            "CFG",
                            "Sampler",
                        },
                    },
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(scriptInfo);
            var deserialized = JsonConvert.DeserializeObject<ScriptInfo>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("XYZ Grid");
            deserialized.Args.Should().HaveCount(1);
            deserialized.Args[0].Label.Should().Be("X axis");
        }

        [Fact]
        public void ExtensionInfo_SerializesCorrectly()
        {
            // Arrange
            var extension = new ExtensionInfo
            {
                Name = "sd-webui-controlnet",
                Remote = "https://github.com/Mikubill/sd-webui-controlnet",
                Branch = "main",
                CommitHash = "abc123def456",
                Version = "v1.0.0",
                CommitDate = 1234567890,
                Enabled = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(extension);
            var deserialized = JsonConvert.DeserializeObject<ExtensionInfo>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("sd-webui-controlnet");
            deserialized.Remote.Should().Contain("github");
            deserialized.Enabled.Should().BeTrue();
        }

        [Fact]
        public void CmdFlags_SerializesCorrectly()
        {
            // Arrange
            var flags = new CmdFlags
            {
                GpuDeviceId = "0",
                AllInFp16 = true,
                DataDir = "/data",
                Share = false,
                Port = "7860",
                Api = true,
                Autolaunch = false,
            };

            // Act
            var json = JsonConvert.SerializeObject(flags);
            var deserialized = JsonConvert.DeserializeObject<CmdFlags>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.GpuDeviceId.Should().Be("0");
            deserialized.AllInFp16.Should().BeTrue();
            deserialized.Api.Should().BeTrue();
        }

        [Fact]
        public void ScriptsList_SerializesCorrectly()
        {
            // Arrange
            var scriptsList = new ScriptsList
            {
                Txt2img = new System.Collections.Generic.List<string>
                {
                    "xyz_grid",
                    "prompts_from_file",
                },
                Img2img = new System.Collections.Generic.List<string>
                {
                    "poor_man_outpainting",
                    "outpainting_mk2",
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(scriptsList);
            var deserialized = JsonConvert.DeserializeObject<ScriptsList>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Txt2img.Should().HaveCount(2);
            deserialized.Img2img.Should().HaveCount(2);
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
