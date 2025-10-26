namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Информация об апскейлере
    /// </summary>
    public class Upscaler
    {
        /// <summary>
        /// Название апскейлера
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Путь к модели (если применимо)
        /// </summary>
        public string? ModelPath { get; set; }

        /// <summary>
        /// URL модели (если применимо)
        /// </summary>
        public string? ModelUrl { get; set; }

        /// <summary>
        /// Масштаб апскейлера
        /// </summary>
        public double? Scale { get; set; }
    }
}
