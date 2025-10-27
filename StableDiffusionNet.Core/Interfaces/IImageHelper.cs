using System.Threading;
using System.Threading.Tasks;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с изображениями
    /// </summary>
    public interface IImageHelper
    {
        /// <summary>
        /// Асинхронно преобразует изображение из файла в base64 строку
        /// </summary>
        /// <param name="filePath">Путь к файлу изображения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Base64 строка с префиксом data:image</returns>
        Task<string> ImageToBase64Async(
            string filePath,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Асинхронно сохраняет base64 строку как файл изображения
        /// </summary>
        /// <param name="base64String">Base64 строка (с или без префикса data:image)</param>
        /// <param name="outputPath">Путь для сохранения файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task Base64ToImageAsync(
            string base64String,
            string outputPath,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Преобразует массив байт в base64 строку
        /// </summary>
        /// <param name="imageBytes">Массив байт изображения</param>
        /// <param name="mimeType">MIME тип (по умолчанию image/png)</param>
        /// <returns>Base64 строка с префиксом data:image</returns>
        string BytesToBase64(byte[] imageBytes, string mimeType = "image/png");

        /// <summary>
        /// Извлекает чистую base64 строку без префикса
        /// </summary>
        /// <param name="base64String">Base64 строка с префиксом или без</param>
        /// <returns>Чистая base64 строка</returns>
        string ExtractBase64Data(string base64String);
    }
}
