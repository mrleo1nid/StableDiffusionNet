using System.Collections.Generic;
using FluentAssertions;
using StableDiffusionNet.Helpers;

namespace StableDiffusionNet.Tests.Helpers
{
    /// <summary>
    /// Тесты для класса Guard
    /// </summary>
    public sealed class GuardTests
    {
        #region ThrowIfNull Tests

        [Fact]
        public void ThrowIfNull_WithNullArgument_ThrowsArgumentNullException()
        {
            // Arrange
            object? nullObject = null;

            // Act
            var act = () => Guard.ThrowIfNull(nullObject, nameof(nullObject));

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("nullObject");
        }

        [Fact]
        public void ThrowIfNull_WithNonNullArgument_DoesNotThrow()
        {
            // Arrange
            var validObject = new object();

            // Act
            var act = () => Guard.ThrowIfNull(validObject, nameof(validObject));

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region ThrowIfNullOrEmpty Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ThrowIfNullOrEmpty_WithNullOrEmpty_ThrowsArgumentException(string? value)
        {
            // Act
            var act = () => Guard.ThrowIfNullOrEmpty(value, nameof(value));

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName(nameof(value))
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        public void ThrowIfNullOrEmpty_WithValidString_DoesNotThrow()
        {
            // Arrange
            var validString = "test";

            // Act
            var act = () => Guard.ThrowIfNullOrEmpty(validString, nameof(validString));

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfNullOrEmpty_WithWhitespace_DoesNotThrow()
        {
            // Arrange
            var whitespaceString = "   ";

            // Act
            var act = () => Guard.ThrowIfNullOrEmpty(whitespaceString, nameof(whitespaceString));

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region ThrowIfNullOrWhiteSpace Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void ThrowIfNullOrWhiteSpace_WithNullOrWhiteSpace_ThrowsArgumentException(
            string? value
        )
        {
            // Act
            var act = () => Guard.ThrowIfNullOrWhiteSpace(value, nameof(value));

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName(nameof(value))
                .WithMessage("*cannot be null, empty, or whitespace*");
        }

        [Fact]
        public void ThrowIfNullOrWhiteSpace_WithValidString_DoesNotThrow()
        {
            // Arrange
            var validString = "test";

            // Act
            var act = () => Guard.ThrowIfNullOrWhiteSpace(validString, nameof(validString));

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region ThrowIfOutOfRange Tests

        [Fact]
        public void ThrowIfOutOfRange_WithValueBelowMin_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var value = 5;
            var min = 10;
            var max = 20;

            // Act
            var act = () => Guard.ThrowIfOutOfRange(value, min, max, nameof(value));

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(nameof(value))
                .WithMessage("*must be between 10 and 20*");
        }

        [Fact]
        public void ThrowIfOutOfRange_WithValueAboveMax_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var value = 25;
            var min = 10;
            var max = 20;

            // Act
            var act = () => Guard.ThrowIfOutOfRange(value, min, max, nameof(value));

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(nameof(value))
                .WithMessage("*must be between 10 and 20*");
        }

        [Theory]
        [InlineData(10)] // min
        [InlineData(15)] // mid
        [InlineData(20)] // max
        public void ThrowIfOutOfRange_WithValueInRange_DoesNotThrow(int value)
        {
            // Arrange
            var min = 10;
            var max = 20;

            // Act
            var act = () => Guard.ThrowIfOutOfRange(value, min, max, nameof(value));

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfOutOfRange_WithDoubles_WorksCorrectly()
        {
            // Arrange
            var value = 5.5;
            var min = 1.0;
            var max = 10.0;

            // Act
            var act = () => Guard.ThrowIfOutOfRange(value, min, max, nameof(value));

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region ThrowIfNegativeOrZero Tests (int)

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ThrowIfNegativeOrZero_Int_WithNegativeOrZero_ThrowsArgumentOutOfRangeException(
            int value
        )
        {
            // Act
            var act = () => Guard.ThrowIfNegativeOrZero(value, nameof(value));

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(nameof(value))
                .WithMessage("*must be greater than zero*");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void ThrowIfNegativeOrZero_Int_WithPositive_DoesNotThrow(int value)
        {
            // Act
            var act = () => Guard.ThrowIfNegativeOrZero(value, nameof(value));

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region ThrowIfNegativeOrZero Tests (long)

        [Theory]
        [InlineData(0L)]
        [InlineData(-1L)]
        [InlineData(-100L)]
        public void ThrowIfNegativeOrZero_Long_WithNegativeOrZero_ThrowsArgumentOutOfRangeException(
            long value
        )
        {
            // Act
            var act = () => Guard.ThrowIfNegativeOrZero(value, nameof(value));

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName(nameof(value))
                .WithMessage("*must be greater than zero*");
        }

        [Theory]
        [InlineData(1L)]
        [InlineData(100L)]
        [InlineData(long.MaxValue)]
        public void ThrowIfNegativeOrZero_Long_WithPositive_DoesNotThrow(long value)
        {
            // Act
            var act = () => Guard.ThrowIfNegativeOrZero(value, nameof(value));

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region ThrowIfEmpty Tests

        [Fact]
        public void ThrowIfEmpty_WithNullCollection_ThrowsArgumentException()
        {
            // Arrange
            IEnumerable<int>? nullCollection = null;

            // Act
            var act = () => Guard.ThrowIfEmpty(nullCollection, nameof(nullCollection));

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("nullCollection")
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        public void ThrowIfEmpty_WithEmptyCollection_ThrowsArgumentException()
        {
            // Arrange
            var emptyCollection = new List<int>();

            // Act
            var act = () => Guard.ThrowIfEmpty(emptyCollection, nameof(emptyCollection));

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("emptyCollection")
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        public void ThrowIfEmpty_WithNonEmptyCollection_DoesNotThrow()
        {
            // Arrange
            var validCollection = new List<int> { 1, 2, 3 };

            // Act
            var act = () => Guard.ThrowIfEmpty(validCollection, nameof(validCollection));

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfEmpty_WithSingleElement_DoesNotThrow()
        {
            // Arrange
            var singleElementCollection = new List<string> { "test" };

            // Act
            var act = () =>
                Guard.ThrowIfEmpty(singleElementCollection, nameof(singleElementCollection));

            // Assert
            act.Should().NotThrow();
        }

        #endregion
    }
}
