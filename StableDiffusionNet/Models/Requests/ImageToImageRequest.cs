using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Requests
{
    /// <summary>
    /// Запрос для генерации изображения из изображения
    /// </summary>
    public class ImageToImageRequest
    {
        /// <summary>
        /// Массив изображений в base64
        /// </summary>
        [JsonProperty("init_images")]
        public List<string> InitImages { get; set; } = new List<string>();

        /// <summary>
        /// Промпт для генерации
        /// </summary>
        [JsonProperty("prompt")]
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Негативный промпт
        /// </summary>
        [JsonProperty("negative_prompt")]
        public string? NegativePrompt { get; set; }

        /// <summary>
        /// Количество шагов сэмплирования
        /// </summary>
        [JsonProperty("steps")]
        public int Steps { get; set; } = 20;

        /// <summary>
        /// Название sampler'а
        /// </summary>
        [JsonProperty("sampler_name")]
        public string? SamplerName { get; set; }

        /// <summary>
        /// Ширина изображения
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; } = 512;

        /// <summary>
        /// Высота изображения
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; } = 512;

        /// <summary>
        /// CFG Scale
        /// </summary>
        [JsonProperty("cfg_scale")]
        public double CfgScale { get; set; } = 7.0;

        /// <summary>
        /// Seed для генерации
        /// </summary>
        [JsonProperty("seed")]
        public long Seed { get; set; } = -1;

        /// <summary>
        /// Denoising strength (0.0-1.0) - сколько изменений внести
        /// </summary>
        [JsonProperty("denoising_strength")]
        public double DenoisingStrength { get; set; } = 0.75;

        /// <summary>
        /// Количество изображений для генерации
        /// </summary>
        [JsonProperty("batch_size")]
        public int BatchSize { get; set; } = 1;

        /// <summary>
        /// Количество батчей
        /// </summary>
        [JsonProperty("n_iter")]
        public int NIter { get; set; } = 1;

        /// <summary>
        /// Восстановление лиц
        /// </summary>
        [JsonProperty("restore_faces")]
        public bool RestoreFaces { get; set; } = false;

        /// <summary>
        /// Tiling
        /// </summary>
        [JsonProperty("tiling")]
        public bool Tiling { get; set; } = false;

        /// <summary>
        /// Режим изменения размера (0: Just resize, 1: Crop and resize, 2: Resize and fill)
        /// </summary>
        [JsonProperty("resize_mode")]
        public int ResizeMode { get; set; } = 0;

        /// <summary>
        /// Маска в base64 (для inpainting)
        /// </summary>
        [JsonProperty("mask")]
        public string? Mask { get; set; }

        /// <summary>
        /// Размытие маски
        /// </summary>
        [JsonProperty("mask_blur")]
        public int MaskBlur { get; set; } = 4;

        /// <summary>
        /// Режим inpainting (0: fill, 1: original, 2: latent noise, 3: latent nothing)
        /// </summary>
        [JsonProperty("inpainting_fill")]
        public int InpaintingFill { get; set; } = 0;

        /// <summary>
        /// Inpaint только в области маски
        /// </summary>
        [JsonProperty("inpaint_full_res")]
        public bool InpaintFullRes { get; set; } = true;

        /// <summary>
        /// Отступ для inpaint области маски
        /// </summary>
        [JsonProperty("inpaint_full_res_padding")]
        public int InpaintFullResPadding { get; set; } = 0;

        /// <summary>
        /// Инвертировать маску
        /// </summary>
        [JsonProperty("inpainting_mask_invert")]
        public int InpaintingMaskInvert { get; set; } = 0;

        /// <summary>
        /// Переопределение настроек
        /// </summary>
        [JsonProperty("override_settings")]
        public Dictionary<string, object>? OverrideSettings { get; set; }

        /// <summary>
        /// Восстановить настройки после генерации
        /// </summary>
        [JsonProperty("override_settings_restore_afterwards")]
        public bool OverrideSettingsRestoreAfterwards { get; set; } = true;
    }
}
