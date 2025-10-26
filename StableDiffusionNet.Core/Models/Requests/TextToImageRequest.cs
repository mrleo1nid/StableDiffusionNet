using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StableDiffusionNet.Models.Requests
{
    /// <summary>
    /// Запрос для генерации изображения из текста
    /// </summary>
    public class TextToImageRequest
    {
        /// <summary>
        /// Максимальный размер изображения
        /// </summary>
        private const int MaxImageSize = 4096;

        /// <summary>
        /// Минимальный размер изображения
        /// </summary>
        private const int MinImageSize = 64;

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
        /// Список стилей промптов для применения
        /// </summary>
        [JsonProperty("styles")]
        public List<string>? Styles { get; set; }

        /// <summary>
        /// Количество шагов сэмплирования (обычно 20-150)
        /// </summary>
        [JsonProperty("steps")]
        public int Steps { get; set; } = 20;

        /// <summary>
        /// Название sampler'а (например: "Euler a", "DPM++ 2M Karras")
        /// </summary>
        [JsonProperty("sampler_name")]
        public string? SamplerName { get; set; }

        /// <summary>
        /// Scheduler (планировщик шагов семплера)
        /// </summary>
        [JsonProperty("scheduler")]
        public string Scheduler { get; set; } = "Automatic";

        /// <summary>
        /// Ширина изображения (должна быть кратна 8)
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; } = 512;

        /// <summary>
        /// Высота изображения (должна быть кратна 8)
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; } = 512;

        /// <summary>
        /// CFG Scale - насколько строго следовать промпту (обычно 7-15)
        /// </summary>
        [JsonProperty("cfg_scale")]
        public double CfgScale { get; set; } = 7.0;

        /// <summary>
        /// Distilled CFG Scale для дистиллированных моделей (Flux и т.д.)
        /// </summary>
        [JsonProperty("distilled_cfg_scale")]
        public double? DistilledCfgScale { get; set; }

        /// <summary>
        /// Seed для генерации (-1 для случайного)
        /// </summary>
        [JsonProperty("seed")]
        public long Seed { get; set; } = -1;

        /// <summary>
        /// Subseed для вариаций (-1 для случайного)
        /// </summary>
        [JsonProperty("subseed")]
        public long Subseed { get; set; } = -1;

        /// <summary>
        /// Сила влияния subseed (0.0-1.0)
        /// </summary>
        [JsonProperty("subseed_strength")]
        public double SubseedStrength { get; set; } = 0;

        /// <summary>
        /// Высота исходного изображения для seed resize (-1 для отключения)
        /// </summary>
        [JsonProperty("seed_resize_from_h")]
        public int SeedResizeFromH { get; set; } = -1;

        /// <summary>
        /// Ширина исходного изображения для seed resize (-1 для отключения)
        /// </summary>
        [JsonProperty("seed_resize_from_w")]
        public int SeedResizeFromW { get; set; } = -1;

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
        /// Tiling (повторяющиеся текстуры)
        /// </summary>
        [JsonProperty("tiling")]
        public bool Tiling { get; set; } = false;

        /// <summary>
        /// Не сохранять сгенерированные изображения на диск
        /// </summary>
        [JsonProperty("do_not_save_samples")]
        public bool DoNotSaveSamples { get; set; } = false;

        /// <summary>
        /// Не сохранять grid изображения на диск
        /// </summary>
        [JsonProperty("do_not_save_grid")]
        public bool DoNotSaveGrid { get; set; } = false;

        /// <summary>
        /// Параметр Eta для DDIM sampler
        /// </summary>
        [JsonProperty("eta")]
        public double? Eta { get; set; }

        /// <summary>
        /// Пропуск негативного промпта на поздних шагах (s_min_uncond)
        /// </summary>
        [JsonProperty("s_min_uncond")]
        public double? SMinUncond { get; set; }

        /// <summary>
        /// Stochasticity parameter (s_churn) для k-diffusion samplers
        /// </summary>
        [JsonProperty("s_churn")]
        public double? SChurn { get; set; }

        /// <summary>
        /// Параметр s_tmax для k-diffusion samplers
        /// </summary>
        [JsonProperty("s_tmax")]
        public double? STmax { get; set; }

        /// <summary>
        /// Параметр s_tmin для k-diffusion samplers
        /// </summary>
        [JsonProperty("s_tmin")]
        public double? STmin { get; set; }

        /// <summary>
        /// Параметр s_noise для k-diffusion samplers
        /// </summary>
        [JsonProperty("s_noise")]
        public double? SNoise { get; set; }

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
        /// Переопределение настроек
        /// </summary>
        [JsonProperty("override_settings")]
        public Dictionary<string, object>? OverrideSettings { get; set; }

        /// <summary>
        /// Восстановить настройки после генерации
        /// </summary>
        [JsonProperty("override_settings_restore_afterwards")]
        public bool OverrideSettingsRestoreAfterwards { get; set; } = true;

        /// <summary>
        /// Скрипты для использования
        /// </summary>
        [JsonProperty("script_args")]
        public List<object>? ScriptArgs { get; set; }

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
        /// Distilled CFG Scale для Hires Fix
        /// </summary>
        [JsonProperty("hr_distilled_cfg")]
        public double? HrDistilledCfg { get; set; }

        /// <summary>
        /// Refiner чекпоинт для SDXL
        /// </summary>
        [JsonProperty("refiner_checkpoint")]
        public string? RefinerCheckpoint { get; set; }

        /// <summary>
        /// Момент переключения на refiner (0.0-1.0)
        /// </summary>
        [JsonProperty("refiner_switch_at")]
        public double? RefinerSwitchAt { get; set; }

        /// <summary>
        /// Отключить extra networks (LoRA, embeddings и т.д.)
        /// </summary>
        [JsonProperty("disable_extra_networks")]
        public bool DisableExtraNetworks { get; set; } = false;

        /// <summary>
        /// Изображение первого прохода (для внутреннего использования)
        /// </summary>
        [JsonProperty("firstpass_image")]
        public string? FirstpassImage { get; set; }

        /// <summary>
        /// Комментарии к генерации
        /// </summary>
        [JsonProperty("comments")]
        public Dictionary<string, object>? Comments { get; set; }

        /// <summary>
        /// Принудительный ID задачи
        /// </summary>
        [JsonProperty("force_task_id")]
        public string? ForceTaskId { get; set; }

        /// <summary>
        /// Устаревший параметр для sampler (используйте sampler_name)
        /// </summary>
        [JsonProperty("sampler_index")]
        public string? SamplerIndex { get; set; }

        /// <summary>
        /// Имя скрипта для выполнения
        /// </summary>
        [JsonProperty("script_name")]
        public string? ScriptName { get; set; }

        /// <summary>
        /// Отправлять изображения в ответе
        /// </summary>
        [JsonProperty("send_images")]
        public bool SendImages { get; set; } = true;

        /// <summary>
        /// Сохранять изображения на диск
        /// </summary>
        [JsonProperty("save_images")]
        public bool SaveImages { get; set; } = false;

        /// <summary>
        /// Параметры для alwayson скриптов (ControlNet, ADetailer и т.д.)
        /// </summary>
        [JsonProperty("alwayson_scripts")]
        public Dictionary<string, object>? AlwaysonScripts { get; set; }

        /// <summary>
        /// Текстовая информация о параметрах генерации (infotext)
        /// </summary>
        [JsonProperty("infotext")]
        public string? Infotext { get; set; }

        /// <summary>
        /// Валидация параметров запроса
        /// </summary>
        /// <param name="paramName">Имя параметра для передачи в исключение</param>
        /// /// <exception cref="ArgumentException">Выбрасывается при невалидных параметрах</exception>
        public void Validate(string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(Prompt))
                throw new ArgumentException("Prompt cannot be empty", paramName ?? nameof(Prompt));

            ValidateImageDimension(Width, nameof(Width));
            ValidateImageDimension(Height, nameof(Height));

            if (Steps <= 0)
                throw new ArgumentException("Steps must be greater than 0", paramName);

            if (CfgScale < 1 || CfgScale > 30)
                throw new ArgumentException("CfgScale must be between 1 and 30", paramName);

            if (BatchSize <= 0)
                throw new ArgumentException("BatchSize must be greater than 0", paramName);

            if (NIter <= 0)
                throw new ArgumentException("NIter must be greater than 0", paramName);

            if (EnableHr && HrScale <= 0)
                throw new ArgumentException(
                    "HrScale must be greater than 0 when EnableHr is true",
                    paramName
                );
        }

        private static void ValidateImageDimension(int value, string dimensionName)
        {
            if (value < MinImageSize || value > MaxImageSize)
                throw new ArgumentException(
                    $"{dimensionName} must be between {MinImageSize} and {MaxImageSize}"
                );

            if (value % 8 != 0)
                throw new ArgumentException($"{dimensionName} must be divisible by 8");
        }
    }
}
