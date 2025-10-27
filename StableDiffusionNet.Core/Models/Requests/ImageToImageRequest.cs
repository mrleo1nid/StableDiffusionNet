using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Requests
{
    /// <summary>
    /// Запрос для генерации изображения из изображения
    /// </summary>
    public class ImageToImageRequest : BaseGenerationRequest
    {
        /// <summary>
        /// Массив изображений в base64
        /// </summary>
        [JsonProperty("init_images")]
        public List<string> InitImages { get; set; } = new List<string>();

        /// <summary>
        /// Denoising strength (0.0-1.0) - сколько изменений внести
        /// </summary>
        [JsonProperty("denoising_strength")]
        public double DenoisingStrength { get; set; } = 0.75;

        /// <summary>
        /// Режим изменения размера (0: Just resize, 1: Crop and resize, 2: Resize and fill)
        /// </summary>
        [JsonProperty("resize_mode")]
        public int ResizeMode { get; set; } = 0;

        /// <summary>
        /// CFG Scale для Image conditioning (ControlNet и т.д.)
        /// </summary>
        [JsonProperty("image_cfg_scale")]
        public double? ImageCfgScale { get; set; }

        /// <summary>
        /// Маска в base64 (для inpainting)
        /// </summary>
        [JsonProperty("mask")]
        public string? Mask { get; set; }

        /// <summary>
        /// Размытие маски по оси X
        /// </summary>
        [JsonProperty("mask_blur_x")]
        public int MaskBlurX { get; set; } = 4;

        /// <summary>
        /// Размытие маски по оси Y
        /// </summary>
        [JsonProperty("mask_blur_y")]
        public int MaskBlurY { get; set; } = 4;

        /// <summary>
        /// Размытие маски (устаревший параметр, используйте mask_blur_x и mask_blur_y)
        /// </summary>
        [JsonProperty("mask_blur")]
        public int? MaskBlur { get; set; }

        /// <summary>
        /// Округлить маску
        /// </summary>
        [JsonProperty("mask_round")]
        public bool MaskRound { get; set; } = true;

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
        /// Множитель начального шума для img2img
        /// </summary>
        [JsonProperty("initial_noise_multiplier")]
        public double? InitialNoiseMultiplier { get; set; }

        /// <summary>
        /// Маска в latent пространстве (для внутреннего использования)
        /// </summary>
        [JsonProperty("latent_mask")]
        public string? LatentMask { get; set; }

        /// <summary>
        /// Включать исходные изображения в результат
        /// </summary>
        [JsonProperty("include_init_images")]
        public bool IncludeInitImages { get; set; } = false;

        /// <summary>
        /// Валидация параметров запроса
        /// </summary>
        /// <param name="paramName">Имя параметра для передачи в исключение</param>
        /// <exception cref="ArgumentException">Выбрасывается при невалидных параметрах</exception>
        public override void Validate(string? paramName = null)
        {
            base.Validate(paramName);

            if (InitImages == null || InitImages.Count == 0)
                throw new ArgumentException(
                    "At least one initial image must be provided",
                    paramName ?? nameof(InitImages)
                );

            if (DenoisingStrength < 0 || DenoisingStrength > 1)
                throw new ArgumentException("DenoisingStrength must be between 0 and 1", paramName);
        }
    }
}
