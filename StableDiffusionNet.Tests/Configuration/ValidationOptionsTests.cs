using FluentAssertions;
using StableDiffusionNet.Configuration;
using Xunit;

namespace StableDiffusionNet.Tests.Configuration
{
    /// <summary>
    /// Тесты для ValidationOptions
    /// </summary>
    public class ValidationOptionsTests
    {
        [Fact]
        public void DefaultValues_ShouldBeValid()
        {
            // Arrange
            var options = new ValidationOptions();

            // Assert
            options.MaxImageSize.Should().Be(4096);
            options.MinImageSize.Should().Be(64);
            options.ImageSizeDivisor.Should().Be(8);
            options.MaxImageFileSize.Should().Be(50 * 1024 * 1024);
            options.MaxJsonLogLength.Should().Be(500);
        }

        [Fact]
        public void Validate_WithValidOptions_DoesNotThrow()
        {
            // Arrange
            var options = new ValidationOptions
            {
                MaxImageSize = 2048,
                MinImageSize = 128,
                ImageSizeDivisor = 8,
                MaxImageFileSize = 25 * 1024 * 1024,
                MaxJsonLogLength = 1000,
            };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNonPositiveMaxImageSize_ThrowsArgumentException(int maxImageSize)
        {
            // Arrange
            var options = new ValidationOptions { MaxImageSize = maxImageSize };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("MaxImageSize must be positive");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNonPositiveMinImageSize_ThrowsArgumentException(int minImageSize)
        {
            // Arrange
            var options = new ValidationOptions { MinImageSize = minImageSize };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("MinImageSize must be positive");
        }

        [Fact]
        public void Validate_WithMinImageSizeGreaterThanMaxImageSize_ThrowsArgumentException()
        {
            // Arrange
            var options = new ValidationOptions { MaxImageSize = 1000, MinImageSize = 2000 };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("MinImageSize cannot be greater than MaxImageSize");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNonPositiveImageSizeDivisor_ThrowsArgumentException(
            int imageSizeDivisor
        )
        {
            // Arrange
            var options = new ValidationOptions { ImageSizeDivisor = imageSizeDivisor };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("ImageSizeDivisor must be positive");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNonPositiveMaxImageFileSize_ThrowsArgumentException(
            long maxImageFileSize
        )
        {
            // Arrange
            var options = new ValidationOptions { MaxImageFileSize = maxImageFileSize };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("MaxImageFileSize must be positive");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNegativeMaxJsonLogLength_ThrowsArgumentException(
            int maxJsonLogLength
        )
        {
            // Arrange
            var options = new ValidationOptions { MaxJsonLogLength = maxJsonLogLength };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("MaxJsonLogLength cannot be negative");
        }

        [Fact]
        public void Validate_WithZeroMaxJsonLogLength_DoesNotThrow()
        {
            // Arrange
            var options = new ValidationOptions { MaxJsonLogLength = 0 };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_WithMultipleInvalidValues_ThrowsFirstException()
        {
            // Arrange
            var options = new ValidationOptions
            {
                MaxImageSize = -1,
                MinImageSize = -2,
                ImageSizeDivisor = -3,
                MaxImageFileSize = -4,
                MaxJsonLogLength = -5,
            };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("MaxImageSize must be positive");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(64)]
        [InlineData(1024)]
        [InlineData(4096)]
        public void Validate_WithValidImageSizes_DoesNotThrow(int imageSize)
        {
            // Arrange
            var options = new ValidationOptions
            {
                MaxImageSize = imageSize,
                MinImageSize = imageSize / 2,
            };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(32)]
        public void Validate_WithValidImageSizeDivisor_DoesNotThrow(int divisor)
        {
            // Arrange
            var options = new ValidationOptions { ImageSizeDivisor = divisor };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [InlineData(10 * 1024 * 1024)]
        [InlineData(100 * 1024 * 1024)]
        [InlineData(long.MaxValue)]
        public void Validate_WithValidMaxImageFileSize_DoesNotThrow(long fileSize)
        {
            // Arrange
            var options = new ValidationOptions { MaxImageFileSize = fileSize };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        public void Validate_WithValidMaxJsonLogLength_DoesNotThrow(int logLength)
        {
            // Arrange
            var options = new ValidationOptions { MaxJsonLogLength = logLength };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }
    }
}
