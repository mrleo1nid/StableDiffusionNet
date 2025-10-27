using FluentAssertions;
using Newtonsoft.Json;
using StableDiffusionNet.Models.Requests;

namespace StableDiffusionNet.Tests.Models.Requests
{
    /// <summary>
    /// Тесты для новых полей в TextToImageRequest
    /// </summary>
    public class TextToImageRequestNewFieldsTests
    {
        [Fact]
        public void NewFields_HaveCorrectDefaultValues()
        {
            // Arrange & Act
            var request = new TextToImageRequest();

            // Assert - Seed параметры
            request.Subseed.Should().Be(-1);
            request.SubseedStrength.Should().Be(0);
            request.SeedResizeFromH.Should().Be(-1);
            request.SeedResizeFromW.Should().Be(-1);

            // Assert - Sampling параметры
            request.DistilledCfgScale.Should().BeNull();
            request.Eta.Should().BeNull();
            request.SMinUncond.Should().BeNull();
            request.SChurn.Should().BeNull();
            request.STmax.Should().BeNull();
            request.STmin.Should().BeNull();
            request.SNoise.Should().BeNull();

            // Assert - Сохранение
            request.DoNotSaveSamples.Should().BeFalse();
            request.DoNotSaveGrid.Should().BeFalse();
            request.SendImages.Should().BeTrue();
            request.SaveImages.Should().BeFalse();

            // Assert - Hires Fix расширенные
            request.FirstphaseWidth.Should().Be(0);
            request.FirstphaseHeight.Should().Be(0);
            request.HrResizeX.Should().Be(0);
            request.HrResizeY.Should().Be(0);
            request.HrScheduler.Should().BeNull();
            request.HrPrompt.Should().BeNull();
            request.HrNegativePrompt.Should().BeNull();
            request.HrCfg.Should().BeNull();
            request.HrDistilledCfg.Should().BeNull();

            // Assert - Refiner
            request.RefinerCheckpoint.Should().BeNull();
            request.RefinerSwitchAt.Should().BeNull();

            // Assert - Скрипты
            request.DisableExtraNetworks.Should().BeFalse();
            request.ScriptName.Should().BeNull();
            request.AlwaysonScripts.Should().BeNull();
        }

        [Fact]
        public void SubseedFields_SerializeCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                Subseed = 12345,
                SubseedStrength = 0.5,
                SeedResizeFromH = 768,
                SeedResizeFromW = 768,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<TextToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Subseed.Should().Be(12345);
            deserialized.SubseedStrength.Should().Be(0.5);
            deserialized.SeedResizeFromH.Should().Be(768);
            deserialized.SeedResizeFromW.Should().Be(768);
        }

        [Fact]
        public void DistilledCfgScale_SerializesCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", DistilledCfgScale = 3.5 };

            // Act
            var json = JsonConvert.SerializeObject(request);

            // Assert
            json.Should().Contain("\"distilled_cfg_scale\":3.5");
        }

        [Fact]
        public void AdvancedSamplingParameters_SerializeCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                Eta = 0.5,
                SMinUncond = 0.3,
                SChurn = 0.1,
                STmax = 0.9,
                STmin = 0.2,
                SNoise = 1.0,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<TextToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Eta.Should().Be(0.5);
            deserialized.SMinUncond.Should().Be(0.3);
            deserialized.SChurn.Should().Be(0.1);
            deserialized.STmax.Should().Be(0.9);
            deserialized.STmin.Should().Be(0.2);
            deserialized.SNoise.Should().Be(1.0);
        }

        [Fact]
        public void HiresFixExtendedParameters_SerializeCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                EnableHr = true,
                FirstphaseWidth = 512,
                FirstphaseHeight = 512,
                HrResizeX = 1024,
                HrResizeY = 1024,
                HrCheckpointName = "model_v2.safetensors",
                HrScheduler = "Karras",
                HrPrompt = "enhanced prompt",
                HrNegativePrompt = "bad quality",
                HrCfg = 8.0,
                HrDistilledCfg = 4.0,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<TextToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.FirstphaseWidth.Should().Be(512);
            deserialized.FirstphaseHeight.Should().Be(512);
            deserialized.HrResizeX.Should().Be(1024);
            deserialized.HrResizeY.Should().Be(1024);
            deserialized.HrCheckpointName.Should().Be("model_v2.safetensors");
            deserialized.HrScheduler.Should().Be("Karras");
            deserialized.HrPrompt.Should().Be("enhanced prompt");
            deserialized.HrNegativePrompt.Should().Be("bad quality");
            deserialized.HrCfg.Should().Be(8.0);
            deserialized.HrDistilledCfg.Should().Be(4.0);
        }

        [Fact]
        public void RefinerParameters_SerializeCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                RefinerCheckpoint = "xl-refiner.safetensors",
                RefinerSwitchAt = 0.8,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<TextToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.RefinerCheckpoint.Should().Be("xl-refiner.safetensors");
            deserialized.RefinerSwitchAt.Should().Be(0.8);
        }

        [Fact]
        public void ScriptParameters_SerializeCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                ScriptName = "xyz_grid",
                AlwaysonScripts = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["controlnet"] = new { enabled = true, model = "control_v11p_sd15_openpose" },
                },
                DisableExtraNetworks = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<TextToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.ScriptName.Should().Be("xyz_grid");
            deserialized.AlwaysonScripts.Should().NotBeNull();
            deserialized.DisableExtraNetworks.Should().BeTrue();
        }

        [Fact]
        public void SaveParameters_SerializeCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                DoNotSaveSamples = true,
                DoNotSaveGrid = true,
                SendImages = false,
                SaveImages = true,
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<TextToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.DoNotSaveSamples.Should().BeTrue();
            deserialized.DoNotSaveGrid.Should().BeTrue();
            deserialized.SendImages.Should().BeFalse();
            deserialized.SaveImages.Should().BeTrue();
        }

        [Fact]
        public void Styles_SerializeCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                Styles = new System.Collections.Generic.List<string> { "cinematic", "anime" },
            };

            // Act
            var json = JsonConvert.SerializeObject(request);
            var deserialized = JsonConvert.DeserializeObject<TextToImageRequest>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Styles.Should().NotBeNull();
            deserialized.Styles.Should().HaveCount(2);
            deserialized.Styles.Should().Contain("cinematic");
            deserialized.Styles.Should().Contain("anime");
        }
    }
}
