using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Опции Stable Diffusion WebUI
    /// Содержит только наиболее используемые настройки
    /// Полный список можно получить через API
    /// </summary>
    public class WebUIOptions
    {
        /// <summary>
        /// Название модели Stable Diffusion
        /// </summary>
        [JsonProperty("sd_model_checkpoint")]
        public string? SdModelCheckpoint { get; set; }

        /// <summary>
        /// Название VAE модели
        /// </summary>
        [JsonProperty("sd_vae")]
        public string? SdVae { get; set; }

        /// <summary>
        /// CLIP skip
        /// </summary>
        [JsonProperty("CLIP_stop_at_last_layers")]
        public double? ClipStopAtLastLayers { get; set; }

        /// <summary>
        /// Включить HR upscaler по умолчанию
        /// </summary>
        [JsonProperty("enable_hr")]
        public bool? EnableHr { get; set; }

        /// <summary>
        /// Название HR upscaler
        /// </summary>
        [JsonProperty("hr_upscaler")]
        public string? HrUpscaler { get; set; }

        /// <summary>
        /// Восстанавливать лица по умолчанию
        /// </summary>
        [JsonProperty("face_restoration_model")]
        public string? FaceRestorationModel { get; set; }

        /// <summary>
        /// Сохранять сэмплы
        /// </summary>
        [JsonProperty("samples_save")]
        public bool? SamplesSave { get; set; }

        /// <summary>
        /// Формат сохранения (png, jpg)
        /// </summary>
        [JsonProperty("samples_format")]
        public string? SamplesFormat { get; set; }

        /// <summary>
        /// Путь для сохранения
        /// </summary>
        [JsonProperty("outdir_samples")]
        public string? OutdirSamples { get; set; }

        /// <summary>
        /// Включить XFORMERS
        /// </summary>
        [JsonProperty("enable_xformers")]
        public bool? EnableXformers { get; set; }
    }
}
