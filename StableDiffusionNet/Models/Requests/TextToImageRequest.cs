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
        /// Seed для генерации (-1 для случайного)
        /// </summary>
        [JsonProperty("seed")]
        public long Seed { get; set; } = -1;

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
    }
}
