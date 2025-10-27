using System;

namespace StableDiffusionNet.Configuration
{
    /// <summary>
    /// Опции валидации для Stable Diffusion клиента.
    /// Позволяет конфигурировать лимиты и правила валидации без перекомпиляции.
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        /// Максимальный размер изображения в пикселях (ширина или высота).
        /// По умолчанию: 4096 (подходит для большинства SD моделей).
        /// </summary>
        public int MaxImageSize { get; set; } = 4096;

        /// <summary>
        /// Минимальный размер изображения в пикселях (ширина или высота).
        /// По умолчанию: 64 (технический минимум для SD моделей).
        /// </summary>
        public int MinImageSize { get; set; } = 64;

        /// <summary>
        /// Делитель размера изображения.
        /// Размеры изображений должны быть кратны этому числу.
        /// По умолчанию: 8 (требование архитектуры Stable Diffusion).
        /// </summary>
        public int ImageSizeDivisor { get; set; } = 8;

        /// <summary>
        /// Максимальный размер файла изображения в байтах.
        /// По умолчанию: 52428800 (50 MB).
        /// </summary>
        public long MaxImageFileSize { get; set; } = 50 * 1024 * 1024;

        /// <summary>
        /// Максимальная длина JSON для логирования без обрезки.
        /// Более длинные JSON обрезаются для предотвращения засорения логов base64 данными.
        /// По умолчанию: 500 символов.
        /// </summary>
        public int MaxJsonLogLength { get; set; } = 500;

        /// <summary>
        /// Валидация опций валидации (inception!)
        /// Проверяет корректность всех установленных значений параметров валидации.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Выбрасывается если:
        /// - MaxImageSize меньше или равен 0
        /// - MinImageSize меньше или равен 0
        /// - MinImageSize больше MaxImageSize
        /// - ImageSizeDivisor меньше или равен 0
        /// - MaxImageFileSize меньше или равен 0
        /// - MaxJsonLogLength меньше 0
        /// </exception>
        public void Validate()
        {
            if (MaxImageSize <= 0)
                throw new ArgumentException("MaxImageSize must be positive");

            if (MinImageSize <= 0)
                throw new ArgumentException("MinImageSize must be positive");

            if (MinImageSize > MaxImageSize)
                throw new ArgumentException("MinImageSize cannot be greater than MaxImageSize");

            if (ImageSizeDivisor <= 0)
                throw new ArgumentException("ImageSizeDivisor must be positive");

            if (MaxImageFileSize <= 0)
                throw new ArgumentException("MaxImageFileSize must be positive");

            if (MaxJsonLogLength < 0)
                throw new ArgumentException("MaxJsonLogLength cannot be negative");
        }
    }
}
