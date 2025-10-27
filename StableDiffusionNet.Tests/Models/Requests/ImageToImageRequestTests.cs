using FluentAssertions;
using StableDiffusionNet.Models.Requests;

namespace StableDiffusionNet.Tests.Models.Requests
{
    /// <summary>
    /// Тесты для ImageToImageRequest
    /// </summary>
    public class ImageToImageRequestTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Act
            var request = new ImageToImageRequest();

            // Assert
            request.InitImages.Should().NotBeNull().And.BeEmpty();
            request.Prompt.Should().Be(string.Empty);
            request.NegativePrompt.Should().BeNull();
            request.Steps.Should().Be(20);
            request.SamplerName.Should().BeNull();
            request.Scheduler.Should().Be("Automatic");
            request.Width.Should().Be(512);
            request.Height.Should().Be(512);
            request.CfgScale.Should().Be(7.0);
            request.Seed.Should().Be(-1);
            request.DenoisingStrength.Should().Be(0.75);
            request.BatchSize.Should().Be(1);
            request.NIter.Should().Be(1);
            request.RestoreFaces.Should().BeFalse();
            request.Tiling.Should().BeFalse();
            request.ResizeMode.Should().Be(0);
            request.Mask.Should().BeNull();
            request.MaskBlurX.Should().Be(4);
            request.MaskBlurY.Should().Be(4);
            request.MaskBlur.Should().BeNull(); // Legacy parameter
            request.MaskRound.Should().BeTrue();
            request.InpaintingFill.Should().Be(0);
            request.InpaintFullRes.Should().BeTrue();
            request.InpaintFullResPadding.Should().Be(0);
            request.InpaintingMaskInvert.Should().Be(0);
            request.OverrideSettingsRestoreAfterwards.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithValidRequest_DoesNotThrow()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "base64image" },
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
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = string.Empty,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*")
                .WithParameterName("Prompt");
        }

        [Fact]
        public void Validate_WithWhitespacePrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "   ",
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("Prompt");
        }

        [Fact]
        public void Validate_WithNullPrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = null!,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Validate_WithNullInitImages_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest { InitImages = null!, Prompt = "test" };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*At least one initial image must be provided*")
                .WithParameterName("InitImages");
        }

        [Fact]
        public void Validate_WithEmptyInitImages_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string>(),
                Prompt = "test",
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*At least one initial image must be provided*");
        }

        [Theory]
        [InlineData(63)]
        [InlineData(4097)]
        public void Validate_WithWidthOutOfRange_ThrowsArgumentException(int width)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Width = width,
            };

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
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Height = height,
            };

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
        public void Validate_WithWidthNotDivisibleBy8_ThrowsArgumentException(int width)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Width = width,
            };

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
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Height = height,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Height must be divisible by 8*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithNonPositiveSteps_ThrowsArgumentException(int steps)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Steps = steps,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Steps must be greater than 0*");
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(31)]
        public void Validate_WithCfgScaleOutOfRange_ThrowsArgumentException(double cfgScale)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                CfgScale = cfgScale,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*CfgScale must be between 1 and 30*");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Validate_WithDenoisingStrengthOutOfRange_ThrowsArgumentException(
            double denoisingStrength
        )
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                DenoisingStrength = denoisingStrength,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*DenoisingStrength must be between 0 and 1*");
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void Validate_WithValidDenoisingStrength_DoesNotThrow(double denoisingStrength)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                DenoisingStrength = denoisingStrength,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithNonPositiveBatchSize_ThrowsArgumentException(int batchSize)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                BatchSize = batchSize,
            };

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
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                NIter = nIter,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*NIter must be greater than 0*");
        }

        [Fact]
        public void InitImages_CanContainMultipleImages()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image1", "image2", "image3" },
                Prompt = "test",
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
            request.InitImages.Should().HaveCount(3);
        }

        [Fact]
        public void AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image1", "image2" },
                Prompt = "test prompt",
                NegativePrompt = "negative",
                Steps = 50,
                SamplerName = "Euler a",
                Scheduler = "Karras",
                Width = 768,
                Height = 768,
                CfgScale = 7.5,
                Seed = 12345,
                DenoisingStrength = 0.5,
                BatchSize = 4,
                NIter = 2,
                RestoreFaces = true,
                Tiling = true,
                ResizeMode = 1,
                Mask = "maskbase64",
                MaskBlur = 8,
                InpaintingFill = 2,
                InpaintFullRes = false,
                InpaintFullResPadding = 32,
                InpaintingMaskInvert = 1,
                OverrideSettings = new Dictionary<string, object> { ["test"] = "value" },
                OverrideSettingsRestoreAfterwards = false,
            };

            // Assert
            request.InitImages.Should().HaveCount(2);
            request.Prompt.Should().Be("test prompt");
            request.NegativePrompt.Should().Be("negative");
            request.Steps.Should().Be(50);
            request.SamplerName.Should().Be("Euler a");
            request.Scheduler.Should().Be("Karras");
            request.Width.Should().Be(768);
            request.Height.Should().Be(768);
            request.CfgScale.Should().Be(7.5);
            request.Seed.Should().Be(12345);
            request.DenoisingStrength.Should().Be(0.5);
            request.BatchSize.Should().Be(4);
            request.NIter.Should().Be(2);
            request.RestoreFaces.Should().BeTrue();
            request.Tiling.Should().BeTrue();
            request.ResizeMode.Should().Be(1);
            request.Mask.Should().Be("maskbase64");
            request.MaskBlur.Should().Be(8);
            request.InpaintingFill.Should().Be(2);
            request.InpaintFullRes.Should().BeFalse();
            request.InpaintFullResPadding.Should().Be(32);
            request.InpaintingMaskInvert.Should().Be(1);
            request.OverrideSettings.Should().ContainKey("test");
            request.OverrideSettingsRestoreAfterwards.Should().BeFalse();
        }

        [Fact]
        public void Validate_WithCustomParamName_UsesCustomName()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "",
                Steps = 0,
            };

            // Act
            var act = () => request.Validate(null, "customParam");

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("customParam");
        }

        [Fact]
        public void Scheduler_DefaultValue_IsAutomatic()
        {
            // Arrange & Act
            var request = new ImageToImageRequest();

            // Assert
            request.Scheduler.Should().Be("Automatic");
        }

        [Fact]
        public void Scheduler_CanBeChanged()
        {
            // Arrange
            var request = new ImageToImageRequest { Scheduler = "Karras" };

            // Assert
            request.Scheduler.Should().Be("Karras");
        }

        [Theory]
        [InlineData(64, 64)]
        [InlineData(512, 512)]
        [InlineData(768, 768)]
        [InlineData(1024, 1024)]
        [InlineData(4096, 4096)]
        public void Validate_WithValidDimensions_DoesNotThrow(int width, int height)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Width = width,
                Height = height,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(150)]
        public void Validate_WithValidSteps_DoesNotThrow(int steps)
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Steps = steps,
            };

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
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                CfgScale = cfgScale,
            };

            // Act
            var act = () => request.Validate();

            // Assert
            act.Should().NotThrow();
        }
    }
}
