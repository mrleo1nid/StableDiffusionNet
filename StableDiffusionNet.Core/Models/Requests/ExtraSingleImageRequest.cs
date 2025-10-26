using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Requests
{
    /// <summary>
    /// Запрос для постобработки одного изображения
    /// </summary>
    public class ExtraSingleImageRequest
    {
        /// <summary>
        /// Base64-закодированное изображение для обработки
        /// </summary>
        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;

        /// <summary>
        /// Фактор масштабирования (resize mode = 0)
        /// </summary>
        [JsonProperty("resize_mode")]
        public int ResizeMode { get; set; } = 0;

        /// <summary>
        /// Показать дополнительные результаты
        /// </summary>
        [JsonProperty("show_extras_results")]
        public bool ShowExtrasResults { get; set; } = true;

        /// <summary>
        /// Включить восстановление лиц с помощью GFPGAN
        /// </summary>
        [JsonProperty("gfpgan_visibility")]
        public double GfpganVisibility { get; set; } = 0;

        /// <summary>
        /// Включить восстановление лиц с помощью CodeFormer
        /// </summary>
        [JsonProperty("codeformer_visibility")]
        public double CodeformerVisibility { get; set; } = 0;

        /// <summary>
        /// Вес CodeFormer
        /// </summary>
        [JsonProperty("codeformer_weight")]
        public double CodeformerWeight { get; set; } = 0.5;

        /// <summary>
        /// Коэффициент увеличения (upscaling_resize)
        /// </summary>
        [JsonProperty("upscaling_resize")]
        public double UpscalingResize { get; set; } = 2;

        /// <summary>
        /// Ширина результата при resize mode = 1
        /// </summary>
        [JsonProperty("upscaling_resize_w")]
        public int? UpscalingResizeW { get; set; }

        /// <summary>
        /// Высота результата при resize mode = 1
        /// </summary>
        [JsonProperty("upscaling_resize_h")]
        public int? UpscalingResizeH { get; set; }

        /// <summary>
        /// Обрезка при resize mode = 1
        /// </summary>
        [JsonProperty("upscaling_crop")]
        public bool? UpscalingCrop { get; set; }

        /// <summary>
        /// Первый апскейлер
        /// </summary>
        [JsonProperty("upscaler_1")]
        public string? Upscaler1 { get; set; }

        /// <summary>
        /// Второй апскейлер (для смешивания)
        /// </summary>
        [JsonProperty("upscaler_2")]
        public string? Upscaler2 { get; set; }

        /// <summary>
        /// Видимость второго апскейлера (0.0 - 1.0)
        /// </summary>
        [JsonProperty("extras_upscaler_2_visibility")]
        public double Upscaler2Visibility { get; set; } = 0;
    }
}
