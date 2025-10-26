using FluentAssertions;
using StableDiffusionNet.Models.Requests;

namespace StableDiffusionNet.Tests.Models.Requests
{
    /// <summary>
    /// Тесты для TextToImageRequest
    /// </summary>
    public class TextToImageRequestTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Act
            var request = new TextToImageRequest();

            // Assert
            request.Prompt.Should().Be(string.Empty);
            request.NegativePrompt.Should().BeNull();
            request.Steps.Should().Be(20);
            request.SamplerName.Should().BeNull();
            request.Scheduler.Should().Be("Automatic");
            request.Width.Should().Be(512);
            request.Height.Should().Be(512);
            request.CfgScale.Should().Be(7.0);
            request.Seed.Should().Be(-1);
            request.BatchSize.Should().Be(1);
            request.NIter.Should().Be(1);
            request.RestoreFaces.Should().BeFalse();
            request.Tiling.Should().BeFalse();
            request.EnableHr.Should().BeFalse();
            request.HrScale.Should().Be(2.0);
            request.OverrideSettingsRestoreAfterwards.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithValidRequest_DoesNotThrow()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test prompt",
                Width = 512,
                Height = 512,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_WithEmptyPrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = string.Empty };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*Prompt cannot be empty*")
                .WithParameterName("Prompt");
        }

        [Fact]
        public void Validate_WithWhitespacePrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "   " };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("Prompt");
        }

        [Fact]
        public void Validate_WithNullPrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = null! };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(63)]
        [InlineData(4097)]
        [InlineData(5000)]
        public void Validate_WithWidthOutOfRange_ThrowsArgumentException(int width)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Width = width };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*Width must be between 64 and 4096*");
        }

        [Theory]
        [InlineData(63)]
        [InlineData(4097)]
        public void Validate_WithHeightOutOfRange_ThrowsArgumentException(int height)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Height = height };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*Height must be between 64 and 4096*");
        }

        [Theory]
        [InlineData(511)]
        [InlineData(513)]
        [InlineData(519)]
        public void Validate_WithWidthNotDivisibleBy8_ThrowsArgumentException(int width)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Width = width };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Width must be divisible by 8*");
        }

        [Theory]
        [InlineData(511)]
        [InlineData(513)]
        public void Validate_WithHeightNotDivisibleBy8_ThrowsArgumentException(int height)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Height = height };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Height must be divisible by 8*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_WithNonPositiveSteps_ThrowsArgumentException(int steps)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Steps = steps };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Steps must be greater than 0*");
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(0.9)]
        [InlineData(31)]
        [InlineData(100)]
        public void Validate_WithCfgScaleOutOfRange_ThrowsArgumentException(double cfgScale)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", CfgScale = cfgScale };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*CfgScale must be between 1 and 30*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithNonPositiveBatchSize_ThrowsArgumentException(int batchSize)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", BatchSize = batchSize };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*BatchSize must be greater than 0*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithNonPositiveNIter_ThrowsArgumentException(int nIter)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", NIter = nIter };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*NIter must be greater than 0*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.5)]
        public void Validate_WithEnableHrAndInvalidHrScale_ThrowsArgumentException(double hrScale)
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                EnableHr = true,
                HrScale = hrScale,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*HrScale must be greater than 0 when EnableHr is true*");
        }

        [Fact]
        public void Validate_WithEnableHrFalseAndZeroHrScale_DoesNotThrow()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                EnableHr = false,
                HrScale = 0,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(64, 64)]
        [InlineData(512, 512)]
        [InlineData(768, 768)]
        [InlineData(1024, 1024)]
        [InlineData(1920, 1080)]
        [InlineData(4096, 4096)]
        public void Validate_WithValidDimensions_DoesNotThrow(int width, int height)
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                Width = width,
                Height = height,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test prompt",
                NegativePrompt = "negative",
                Steps = 50,
                SamplerName = "Euler a",
                Scheduler = "Karras",
                Width = 768,
                Height = 768,
                CfgScale = 7.5,
                Seed = 12345,
                BatchSize = 4,
                NIter = 2,
                RestoreFaces = true,
                Tiling = true,
                EnableHr = true,
                HrScale = 2.5,
                HrUpscaler = "Latent",
                HrSecondPassSteps = 20,
                DenoisingStrength = 0.5,
                OverrideSettings = new Dictionary<string, object> { ["test"] = "value" },
                OverrideSettingsRestoreAfterwards = false,
                ScriptArgs = new List<object> { "arg1", 123 },
                HrSamplerName = "DPM++ 2M",
            };

            // Assert
            request.Prompt.Should().Be("test prompt");
            request.NegativePrompt.Should().Be("negative");
            request.Steps.Should().Be(50);
            request.SamplerName.Should().Be("Euler a");
            request.Scheduler.Should().Be("Karras");
            request.Width.Should().Be(768);
            request.Height.Should().Be(768);
            request.CfgScale.Should().Be(7.5);
            request.Seed.Should().Be(12345);
            request.BatchSize.Should().Be(4);
            request.NIter.Should().Be(2);
            request.RestoreFaces.Should().BeTrue();
            request.Tiling.Should().BeTrue();
            request.EnableHr.Should().BeTrue();
            request.HrScale.Should().Be(2.5);
            request.HrUpscaler.Should().Be("Latent");
            request.HrSecondPassSteps.Should().Be(20);
            request.DenoisingStrength.Should().Be(0.5);
            request.OverrideSettings.Should().ContainKey("test");
            request.OverrideSettingsRestoreAfterwards.Should().BeFalse();
            request.ScriptArgs.Should().HaveCount(2);
            request.HrSamplerName.Should().Be("DPM++ 2M");
        }

        [Fact]
        public void Validate_WithCustomParamName_UsesCustomName()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "", Steps = 0 };

            // Act
            var act = () => request.Validate("customParam");

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("customParam");
        }

        [Fact]
        public void Scheduler_DefaultValue_IsAutomatic()
        {
            // Arrange & Act
            var request = new TextToImageRequest();

            // Assert
            request.Scheduler.Should().Be("Automatic");
        }

        [Fact]
        public void Scheduler_CanBeChanged()
        {
            // Arrange
            var request = new TextToImageRequest { Scheduler = "Karras" };

            // Assert
            request.Scheduler.Should().Be("Karras");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(150)]
        public void Validate_WithValidSteps_DoesNotThrow(int steps)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Steps = steps };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(7.0)]
        [InlineData(15.0)]
        [InlineData(30.0)]
        public void Validate_WithValidCfgScale_DoesNotThrow(double cfgScale)
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", CfgScale = cfgScale };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }
    }
}
