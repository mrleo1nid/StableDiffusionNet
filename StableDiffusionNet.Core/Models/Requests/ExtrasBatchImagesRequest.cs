using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Requests
{
    /// <summary>
    /// Запрос для постобработки батча изображений
    /// </summary>
    public class ExtrasBatchImagesRequest
    {
        /// <summary>
        /// Список изображений в base64 для обработки
        /// </summary>
        [JsonProperty("imageList")]
        public List<FileData> ImageList { get; set; } = new List<FileData>();

        /// <summary>
        /// Режим изменения размера (0: upscale by upscaling_resize, 1: upscale to upscaling_resize_w x upscaling_resize_h)
        /// </summary>
        [JsonProperty("resize_mode")]
        public int ResizeMode { get; set; } = 0;

        /// <summary>
        /// Показать дополнительные результаты
        /// </summary>
        [JsonProperty("show_extras_results")]
        public bool ShowExtrasResults { get; set; } = true;

        /// <summary>
        /// Включить восстановление лиц с помощью GFPGAN (0.0 - 1.0)
        /// </summary>
        [JsonProperty("gfpgan_visibility")]
        public double GfpganVisibility { get; set; } = 0;

        /// <summary>
        /// Включить восстановление лиц с помощью CodeFormer (0.0 - 1.0)
        /// </summary>
        [JsonProperty("codeformer_visibility")]
        public double CodeformerVisibility { get; set; } = 0;

        /// <summary>
        /// Вес CodeFormer (0.0 - 1.0)
        /// </summary>
        [JsonProperty("codeformer_weight")]
        public double CodeformerWeight { get; set; } = 0;

        /// <summary>
        /// Коэффициент увеличения (только для resize_mode = 0)
        /// </summary>
        [JsonProperty("upscaling_resize")]
        public double UpscalingResize { get; set; } = 2;

        /// <summary>
        /// Целевая ширина (только для resize_mode = 1)
        /// </summary>
        [JsonProperty("upscaling_resize_w")]
        public int UpscalingResizeW { get; set; } = 512;

        /// <summary>
        /// Целевая высота (только для resize_mode = 1)
        /// </summary>
        [JsonProperty("upscaling_resize_h")]
        public int UpscalingResizeH { get; set; } = 512;

        /// <summary>
        /// Обрезать изображение до целевого размера
        /// </summary>
        [JsonProperty("upscaling_crop")]
        public bool UpscalingCrop { get; set; } = true;

        /// <summary>
        /// Первый апскейлер
        /// </summary>
        [JsonProperty("upscaler_1")]
        public string Upscaler1 { get; set; } = "None";

        /// <summary>
        /// Второй апскейлер (для смешивания)
        /// </summary>
        [JsonProperty("upscaler_2")]
        public string Upscaler2 { get; set; } = "None";

        /// <summary>
        /// Видимость второго апскейлера (0.0 - 1.0)
        /// </summary>
        [JsonProperty("extras_upscaler_2_visibility")]
        public double Upscaler2Visibility { get; set; } = 0;

        /// <summary>
        /// Выполнять апскейл перед восстановлением лиц
        /// </summary>
        [JsonProperty("upscale_first")]
        public bool UpscaleFirst { get; set; } = false;
    }

    /// <summary>
    /// Данные файла для пакетной обработки
    /// </summary>
    public class FileData
    {
        /// <summary>
        /// Base64 представление файла
        /// </summary>
        [JsonProperty("data")]
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Имя файла
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
