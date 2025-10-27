using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StableDiffusionNet.Configuration;

namespace StableDiffusionNet.Models.Requests
{
    /// <summary>
    /// Запрос для генерации изображения из текста
    /// </summary>
    public class TextToImageRequest : BaseGenerationRequest
    {
        /// <summary>
        /// Включить высокое разрешение
        /// </summary>
        [JsonProperty("enable_hr")]
        public bool EnableHr { get; set; } = false;

        /// <summary>
        /// Масштаб для высокого разрешения
        /// </summary>
        [JsonProperty("hr_scale")]
        public double HrScale { get; set; } = 2.0;

        /// <summary>
        /// Upscaler для высокого разрешения
        /// </summary>
        [JsonProperty("hr_upscaler")]
        public string? HrUpscaler { get; set; }

        /// <summary>
        /// Количество шагов для высокого разрешения
        /// </summary>
        [JsonProperty("hr_second_pass_steps")]
        public int? HrSecondPassSteps { get; set; }

        /// <summary>
        /// Denoising strength для высокого разрешения
        /// </summary>
        [JsonProperty("denoising_strength")]
        public double? DenoisingStrength { get; set; }

        /// <summary>
        /// Включить HR sampler
        /// </summary>
        [JsonProperty("hr_sampler_name")]
        public string? HrSamplerName { get; set; }

        /// <summary>
        /// Scheduler для Hires Fix
        /// </summary>
        [JsonProperty("hr_scheduler")]
        public string? HrScheduler { get; set; }

        /// <summary>
        /// Ширина первого прохода (0 для автоматического расчета)
        /// </summary>
        [JsonProperty("firstphase_width")]
        public int FirstphaseWidth { get; set; } = 0;

        /// <summary>
        /// Высота первого прохода (0 для автоматического расчета)
        /// </summary>
        [JsonProperty("firstphase_height")]
        public int FirstphaseHeight { get; set; } = 0;

        /// <summary>
        /// Целевая ширина для Hires Fix (0 для использования hr_scale)
        /// </summary>
        [JsonProperty("hr_resize_x")]
        public int HrResizeX { get; set; } = 0;

        /// <summary>
        /// Целевая высота для Hires Fix (0 для использования hr_scale)
        /// </summary>
        [JsonProperty("hr_resize_y")]
        public int HrResizeY { get; set; } = 0;

        /// <summary>
        /// Чекпоинт для Hires Fix (если отличается от основного)
        /// </summary>
        [JsonProperty("hr_checkpoint_name")]
        public string? HrCheckpointName { get; set; }

        /// <summary>
        /// Дополнительные модули для Hires Fix (LoRA и т.д.)
        /// </summary>
        [JsonProperty("hr_additional_modules")]
        public List<object>? HrAdditionalModules { get; set; }

        /// <summary>
        /// Промпт для Hires Fix (если отличается от основного)
        /// </summary>
        [JsonProperty("hr_prompt")]
        public string? HrPrompt { get; set; }

        /// <summary>
        /// Негативный промпт для Hires Fix (если отличается от основного)
        /// </summary>
        [JsonProperty("hr_negative_prompt")]
        public string? HrNegativePrompt { get; set; }

        /// <summary>
        /// CFG Scale для Hires Fix
        /// </summary>
        [JsonProperty("hr_cfg")]
        public double? HrCfg { get; set; }

        /// <summary>
        /// Валидация параметров запроса
        /// </summary>
        /// <param name="validationOptions">Опции валидации (если null - используются значения по умолчанию)</param>
        /// <param name="paramName">Имя параметра для передачи в исключение</param>
        /// <exception cref="ArgumentException">Выбрасывается при невалидных параметрах</exception>
        public override void Validate(
            ValidationOptions? validationOptions = null,
            string? paramName = null
        )
        {
            base.Validate(validationOptions, paramName);

            if (EnableHr && HrScale <= 0)
                throw new ArgumentException(
                    "HrScale must be greater than 0 when EnableHr is true",
                    paramName
                );
        }
    }
}
