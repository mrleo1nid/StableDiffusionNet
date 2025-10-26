using System;
using System.IO;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Вспомогательные методы для работы с изображениями
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Преобразует изображение из файла в base64 строку
        /// </summary>
        /// <param name="filePath">Путь к файлу изображения</param>
        /// <returns>Base64 строка с префиксом data:image</returns>
        public static string ImageToBase64(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var bytes = File.ReadAllBytes(filePath);
            var base64 = Convert.ToBase64String(bytes);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            var mimeType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "image/png",
            };

            return $"data:{mimeType};base64,{base64}";
        }

        /// <summary>
        /// Сохраняет base64 строку как файл изображения
        /// </summary>
        /// <param name="base64String">Base64 строка (с или без префикса data:image)</param>
        /// <param name="outputPath">Путь для сохранения файла</param>
        public static void Base64ToImage(string base64String, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                throw new ArgumentException("Base64 string cannot be empty", nameof(base64String));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

            // Убираем префикс data:image/xxx;base64, если он есть
            var base64Data = base64String;
            if (base64String.Contains(","))
            {
                base64Data = base64String.Substring(
                    base64String.IndexOf(",", StringComparison.Ordinal) + 1
                );
            }

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
        public static string BytesToBase64(byte[] imageBytes, string mimeType = "image/png")
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
        public static string ExtractBase64Data(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                throw new ArgumentException("Base64 string cannot be empty", nameof(base64String));

            if (base64String.Contains(","))
            {
                return base64String.Substring(
                    base64String.IndexOf(",", StringComparison.Ordinal) + 1
                );
            }

            return base64String;
        }
    }
}
