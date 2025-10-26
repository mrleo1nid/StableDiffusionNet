namespace StableDiffusionNet.Models.Responses
{
    /// <summary>
    /// Ответ постобработки изображения
    /// </summary>
    public class ExtraSingleImageResponse
    {
        /// <summary>
        /// Base64-закодированное обработанное изображение
        /// </summary>
        public string Image { get; set; } = string.Empty;

        /// <summary>
        /// HTML с дополнительными результатами (если запрошено)
        /// </summary>
        public string? Html_info { get; set; }
    }
}
