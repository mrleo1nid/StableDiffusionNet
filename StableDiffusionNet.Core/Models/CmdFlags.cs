using Newtonsoft.Json;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Флаги командной строки WebUI
    /// </summary>
    public class CmdFlags
    {
        /// <summary>
        /// ID GPU устройства
        /// </summary>
        [JsonProperty("gpu_device_id")]
        public string? GpuDeviceId { get; set; }

        /// <summary>
        /// Использовать FP32 для всего
        /// </summary>
        [JsonProperty("all_in_fp32")]
        public bool AllInFp32 { get; set; }

        /// <summary>
        /// Использовать FP16 для всего
        /// </summary>
        [JsonProperty("all_in_fp16")]
        public bool AllInFp16 { get; set; }

        /// <summary>
        /// Директория с данными
        /// </summary>
        [JsonProperty("data_dir")]
        public string DataDir { get; set; } = string.Empty;

        /// <summary>
        /// Использовать share через Gradio
        /// </summary>
        [JsonProperty("share")]
        public bool Share { get; set; }

        /// <summary>
        /// Порт сервера
        /// </summary>
        [JsonProperty("port")]
        public string? Port { get; set; }

        /// <summary>
        /// Включить API
        /// </summary>
        [JsonProperty("api")]
        public bool Api { get; set; }

        /// <summary>
        /// Автоматически запускать браузер
        /// </summary>
        [JsonProperty("autolaunch")]
        public bool Autolaunch { get; set; }

        // Можно добавить остальные флаги по необходимости
    }
}
