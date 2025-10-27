using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Вспомогательные методы для работы с изображениями
    /// </summary>
    public class ImageHelper : IImageHelper
    {
        /// <summary>
        /// Публичный конструктор для использования с DI
        /// </summary>
        public ImageHelper() { }

        /// <summary>
        /// Максимальный размер файла изображения в байтах (50 МБ).
        /// /// Stable Diffusion WebUI обычно генерирует изображения до 10-20 МБ.
        /// 50 МБ - запас для изображений высокого разрешения с минимальным сжатием
        /// или для работы с несколькими изображениями одновременно.
        /// </summary>
        private const long MaxFileSize = 50 * 1024 * 1024;

        // MIME типы для изображений
        private const string MimeTypePng = "image/png";
        private const string MimeTypeJpeg = "image/jpeg";
        private const string MimeTypeGif = "image/gif";
        private const string MimeTypeWebp = "image/webp";
        private const string MimeTypeBmp = "image/bmp";

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
            if (string.IsNullOrWhiteSpace(base64String))
                throw new ArgumentException("Base64 string cannot be empty", nameof(base64String));

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
        /// Асинхронно преобразует изображение из файла в base64 строку
        /// </summary>
        /// <param name="filePath">Путь к файлу изображения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Base64 строка с префиксом data:image</returns>
        /// <exception cref="ArgumentException">Выбрасывается если файл слишком большой</exception>
        public async Task<string> ImageToBase64Async(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MaxFileSize)
                throw new ArgumentException(
                    $"File size ({fileInfo.Length} bytes) exceeds maximum allowed size ({MaxFileSize} bytes)",
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

            var base64 = Convert.ToBase64String(bytes);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            var mimeType = extension switch
            {
                ".png" => MimeTypePng,
                ".jpg" or ".jpeg" => MimeTypeJpeg,
                ".gif" => MimeTypeGif,
                ".webp" => MimeTypeWebp,
                ".bmp" => MimeTypeBmp,
                _ => MimeTypePng,
            };

            return $"data:{mimeType};base64,{base64}";
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
            if (string.IsNullOrWhiteSpace(base64String))
                throw new ArgumentException("Base64 string cannot be empty", nameof(base64String));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

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
    }
}
