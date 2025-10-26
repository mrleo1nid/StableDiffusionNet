using System;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Валидатор для запросов генерации изображений
    /// </summary>
    internal static class ImageRequestValidator
    {
        /// <summary>
        /// Максимальный размер изображения
        /// </summary>
        private const int MaxImageSize = 4096;

        /// <summary>
        /// Минимальный размер изображения
        /// </summary>
        private const int MinImageSize = 64;

        /// <summary>
        /// Валидирует размер изображения
        /// </summary>
        /// <param name="value">Значение размера</param>
        /// <param name="dimensionName">Имя измерения (Width или Height)</param>
        /// <exception cref="ArgumentException">Выбрасывается при невалидном размере</exception>
        public static void ValidateImageDimension(int value, string dimensionName)
        {
            if (value < MinImageSize || value > MaxImageSize)
                throw new ArgumentException(
                    $"{dimensionName} must be between {MinImageSize} and {MaxImageSize}"
                );

            if (value % 8 != 0)
                throw new ArgumentException($"{dimensionName} must be divisible by 8");
        }
    }
}
