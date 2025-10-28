using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Вспомогательные методы для работы с изображениями.
    /// Использует конфигурируемые лимиты вместо хардкоженных констант.
    /// Включает валидацию формата по magic bytes для безопасности.
    /// </summary>
    public class ImageHelper : IImageHelper
    {
        private readonly ValidationOptions _validationOptions;

        /// <summary>
        /// Конструктор по умолчанию для обратной совместимости.
        /// Использует стандартные опции валидации.
        /// </summary>
        public ImageHelper()
            : this(new ValidationOptions()) { }

        /// <summary>
        /// Конструктор с опциями валидации для использования с DI
        /// </summary>
        /// <param name="validationOptions">Опции валидации</param>
        public ImageHelper(ValidationOptions validationOptions)
        {
            _validationOptions =
                validationOptions ?? throw new ArgumentNullException(nameof(validationOptions));
        }

        // MIME типы для изображений
        private const string MimeTypePng = "image/png";
        private const string MimeTypeJpeg = "image/jpeg";
        private const string MimeTypeGif = "image/gif";
        private const string MimeTypeWebp = "image/webp";
        private const string MimeTypeBmp = "image/bmp";

        /// <summary>
        /// Magic bytes (сигнатуры файлов) для различных форматов изображений.
        /// Используется для определения реального формата файла, а не по расширению.
        /// </summary>
        private static readonly Dictionary<string, byte[][]> ImageSignatures = new Dictionary<
            string,
            byte[][]
        >
        {
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            [MimeTypePng] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            // JPEG: FF D8 FF (различные маркеры)
            [MimeTypeJpeg] = new[]
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JFIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // EXIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // Canon
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // Samsung
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, // SPIFF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, // JPEG raw
            },
            // GIF: GIF87a или GIF89a
            [MimeTypeGif] = new[]
            {
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, // GIF87a
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, // GIF89a
            },
            // WebP: RIFF....WEBP
            [MimeTypeWebp] = new[]
            {
                new byte[] { 0x52, 0x49, 0x46, 0x46 }, // RIFF (нужна дополнительная проверка WEBP)
            },
            // BMP: BM
            [MimeTypeBmp] = new[] { new byte[] { 0x42, 0x4D } }, // BM
        };

        /// <summary>
        /// Преобразует массив байт в base64 строку
        /// </summary>
        /// <param name="imageBytes">Массив байт изображения</param>
        /// <param name="mimeType">MIME тип (по умолчанию image/png)</param>
        /// <returns>Base64 строка с префиксом data:image</returns>
        public string BytesToBase64(byte[] imageBytes, string mimeType = "image/png")
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Byte array cannot be empty", nameof(imageBytes));

            var base64 = Convert.ToBase64String(imageBytes);
            return $"data:{mimeType};base64,{base64}";
        }

        /// <summary>
        /// Извлекает чистую base64 строку без префикса
        /// </summary>
        /// <param name="base64String">Base64 строка с префиксом или без</param>
        /// <returns>Чистая base64 строка</returns>
        public string ExtractBase64Data(string base64String)
        {
            Guard.ThrowIfNullOrWhiteSpace(base64String);

            var commaIndex = base64String.IndexOf(',');
            if (commaIndex >= 0)
            {
#if NETSTANDARD2_0
                return base64String.Substring(commaIndex + 1);
#else
                return base64String[(commaIndex + 1)..];
#endif
            }

            return base64String;
        }

        /// <summary>
        /// Определяет формат изображения по magic bytes (сигнатуре файла).
        /// Это более надежный способ определения формата, чем по расширению файла.
        /// </summary>
        /// <param name="bytes">Массив байт изображения</param>
        /// <returns>MIME тип изображения или null, если формат не распознан</returns>
        private static string? DetectImageFormat(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 12)
                return null;

            foreach (var kvp in ImageSignatures)
            {
                var detectedMimeType = TryMatchSignature(bytes, kvp.Key, kvp.Value);
                if (detectedMimeType != null)
                    return detectedMimeType;
            }

            return null;
        }

        /// <summary>
        /// Пытается найти совпадение с одной из сигнатур для указанного MIME типа
        /// </summary>
        private static string? TryMatchSignature(byte[] bytes, string mimeType, byte[][] signatures)
        {
            if (!signatures.Any(sig => SignatureMatches(bytes, sig)))
                return null;

            return mimeType == MimeTypeWebp ? ValidateWebP(bytes, mimeType) : mimeType;
        }

        /// <summary>
        /// Проверяет соответствие байтов сигнатуре
        /// </summary>
        private static bool SignatureMatches(byte[] bytes, byte[] signature)
        {
            if (bytes.Length < signature.Length)
                return false;

            for (int i = 0; i < signature.Length; i++)
            {
                if (bytes[i] != signature[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Дополнительная валидация для WebP формата (проверка RIFF + WEBP маркеров)
        /// </summary>
        private static string? ValidateWebP(byte[] bytes, string mimeType)
        {
            // RIFF формат: проверяем наличие "WEBP" на позиции 8-11
            const int minWebPLength = 12;
            const int webpMarkerStart = 8;

            if (bytes.Length < minWebPLength)
                return null;

            // Проверяем "WEBP" маркер
            var webpMarker = new byte[] { 0x57, 0x45, 0x42, 0x50 }; // "WEBP"
            for (int i = 0; i < webpMarker.Length; i++)
            {
                if (bytes[webpMarkerStart + i] != webpMarker[i])
                    return null;
            }

            return mimeType;
        }

        /// <summary>
        /// Асинхронно преобразует изображение из файла в base64 строку.
        /// Валидирует формат файла по magic bytes для безопасности.
        /// </summary>
        /// <param name="filePath">Путь к файлу изображения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Base64 строка с префиксом data:image</returns>
        /// <exception cref="ArgumentException">Выбрасывается если файл слишком большой или не является валидным изображением</exception>
        /// <exception cref="FileNotFoundException">Выбрасывается если файл не найден</exception>
        public async Task<string> ImageToBase64Async(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            Guard.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > _validationOptions.MaxImageFileSize)
                throw new ArgumentException(
                    $"File size ({fileInfo.Length} bytes) exceeds maximum allowed size ({_validationOptions.MaxImageFileSize} bytes)",
                    nameof(filePath)
                );

            byte[] bytes;
#if NETSTANDARD2_0 || NETSTANDARD2_1
            // .NET Standard 2.0/2.1 не имеет File.ReadAllBytesAsync
            bytes = await Task.Run(() => File.ReadAllBytes(filePath), cancellationToken)
                .ConfigureAwait(false);
#else
            bytes = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
#endif

            // Определяем реальный формат файла по magic bytes (не по расширению!)
            var detectedMimeType = DetectImageFormat(bytes);
            if (detectedMimeType == null)
            {
                throw new ArgumentException(
                    "File is not a valid image format. Supported formats: PNG, JPEG, GIF, WebP, BMP",
                    nameof(filePath)
                );
            }

            var base64 = Convert.ToBase64String(bytes);
            return $"data:{detectedMimeType};base64,{base64}";
        }

        /// <summary>
        /// Асинхронно сохраняет base64 строку как файл изображения
        /// </summary>
        /// <param name="base64String">Base64 строка (с или без префикса data:image)</param>
        /// <param name="outputPath">Путь для сохранения файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public async Task Base64ToImageAsync(
            string base64String,
            string outputPath,
            CancellationToken cancellationToken = default
        )
        {
            Guard.ThrowIfNullOrWhiteSpace(base64String);
            Guard.ThrowIfNullOrWhiteSpace(outputPath);

            var base64Data = ExtractBase64Data(base64String);

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(
                    "Invalid base64 string format",
                    nameof(base64String),
                    ex
                );
            }

            // Создаем директорию если не существует
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

#if NETSTANDARD2_0 || NETSTANDARD2_1
            // .NET Standard 2.0/2.1 не имеет File.WriteAllBytesAsync
            await Task.Run(() => File.WriteAllBytes(outputPath, bytes), cancellationToken)
                .ConfigureAwait(false);
#else
            await File.WriteAllBytesAsync(outputPath, bytes, cancellationToken)
                .ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Валидирует что base64 строка содержит валидное изображение.
        /// Проверяет формат по magic bytes, а не только корректность base64.
        /// </summary>
        /// <param name="base64String">Base64 строка (с или без префикса data:image)</param>
        /// <exception cref="ArgumentException">Выбрасывается если base64 некорректен или не содержит валидное изображение</exception>
        public void ValidateImageBase64(string base64String)
        {
            Guard.ThrowIfNullOrWhiteSpace(base64String);

            var base64Data = ExtractBase64Data(base64String);

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(
                    "Invalid base64 string format",
                    nameof(base64String),
                    ex
                );
            }

            var detectedMimeType = DetectImageFormat(bytes);
            if (detectedMimeType == null)
            {
                throw new ArgumentException(
                    "Base64 data does not contain a valid image. Supported formats: PNG, JPEG, GIF, WebP, BMP",
                    nameof(base64String)
                );
            }
        }
    }
}
