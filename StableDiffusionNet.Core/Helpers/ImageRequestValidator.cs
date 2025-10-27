using System;
using StableDiffusionNet.Configuration;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Валидатор для запросов генерации изображений.
    /// Использует конфигурируемые опции валидации вместо хардкоженных констант.
    /// </summary>
    internal class ImageRequestValidator
    {
        private readonly ValidationOptions _validationOptions;

        /// <summary>
        /// Создает новый экземпляр валидатора
        /// </summary>
        /// <param name="validationOptions">Опции валидации</param>
        public ImageRequestValidator(ValidationOptions validationOptions)
        {
            _validationOptions =
                validationOptions ?? throw new ArgumentNullException(nameof(validationOptions));
        }

        /// <summary>
        /// Валидирует размер изображения
        /// </summary>
        /// <param name="value">Значение размера</param>
        /// <param name="dimensionName">Имя измерения (Width или Height)</param>
        /// <exception cref="ArgumentException">Выбрасывается при невалидном размере</exception>
        public void ValidateImageDimension(int value, string dimensionName)
        {
            if (value < _validationOptions.MinImageSize || value > _validationOptions.MaxImageSize)
                throw new ArgumentException(
                    $"{dimensionName} must be between {_validationOptions.MinImageSize} and {_validationOptions.MaxImageSize}"
                );

            if (value % _validationOptions.ImageSizeDivisor != 0)
                throw new ArgumentException(
                    $"{dimensionName} must be divisible by {_validationOptions.ImageSizeDivisor}"
                );
        }
    }
}
