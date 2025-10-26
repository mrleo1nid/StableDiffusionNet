using System;
using System.IO;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Вспомогательные методы для работы с изображениями
    /// </summary>
    public class ImageHelper : IImageHelper
    {
        /// <summary>
        /// Singleton экземпляр для использования без DI
        /// </summary>
        public static readonly ImageHelper Instance = new ImageHelper();

        /// <summary>
        /// Публичный конструктор для использования с DI
        /// </summary>
        public ImageHelper() { }

        /// <summary>
        /// Максимальный размер файла изображения в байтах (50 МБ)
        /// </summary>
        private const long MaxFileSize = 50 * 1024 * 1024;

        // MIME типы для изображений
        private const string MimeTypePng = "image/png";
        private const string MimeTypeJpeg = "image/jpeg";
        private const string MimeTypeGif = "image/gif";
        private const string MimeTypeWebp = "image/webp";
        private const string MimeTypeBmp = "image/bmp";

        /// <summary>
        /// Преобразует изображение из файла в base64 строку
        /// </summary>
        /// <param name="filePath">Путь к файлу изображения</param>
        /// <returns>Base64 строка с префиксом data:image</returns>
        /// <exception cref="ArgumentException">Выбрасывается если файл слишком большой</exception>
        public string ImageToBase64(string filePath)
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

            var bytes = File.ReadAllBytes(filePath);
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
        /// Сохраняет base64 строку как файл изображения
        /// </summary>
        /// <param name="base64String">Base64 строка (с или без префикса data:image)</param>
        /// <param name="outputPath">Путь для сохранения файла</param>
        public void Base64ToImage(string base64String, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                throw new ArgumentException("Base64 string cannot be empty", nameof(base64String));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

            var base64Data = ExtractBase64Data(base64String);
            var bytes = Convert.FromBase64String(base64Data);

            // Создаем директорию если не существует
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(outputPath, bytes);
        }

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
    }
}
