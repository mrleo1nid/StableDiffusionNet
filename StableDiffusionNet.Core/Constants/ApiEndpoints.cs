namespace StableDiffusionNet.Constants
{
    /// <summary>
    /// Константы для эндпоинтов Stable Diffusion WebUI API
    /// </summary>
    internal static class ApiEndpoints
    {
        /// <summary>
        /// Базовый префикс для всех API эндпоинтов
        /// </summary>
        public const string ApiPrefix = "/sdapi/v1";

        /// <summary>
        /// Эндпоинт для генерации изображений из текста (txt2img)
        /// </summary>
        public const string TextToImage = $"{ApiPrefix}/txt2img";

        /// <summary>
        /// Эндпоинт для генерации изображений из изображений (img2img)
        /// </summary>
        public const string ImageToImage = $"{ApiPrefix}/img2img";

        /// <summary>
        /// Эндпоинт для получения списка доступных моделей
        /// </summary>
        public const string Models = $"{ApiPrefix}/sd-models";

        /// <summary>
        /// Эндпоинт для получения и изменения настроек WebUI
        /// </summary>
        public const string Options = $"{ApiPrefix}/options";

        /// <summary>
        /// Эндпоинт для обновления списка чекпоинтов моделей
        /// </summary>
        public const string RefreshCheckpoints = $"{ApiPrefix}/refresh-checkpoints";

        /// <summary>
        /// Эндпоинт для получения прогресса генерации
        /// </summary>
        public const string Progress = $"{ApiPrefix}/progress";

        /// <summary>
        /// Эндпоинт для прерывания текущей генерации
        /// </summary>
        public const string Interrupt = $"{ApiPrefix}/interrupt";

        /// <summary>
        /// Эндпоинт для пропуска текущего изображения в батче
        /// </summary>
        public const string Skip = $"{ApiPrefix}/skip";

        /// <summary>
        /// Эндпоинт для получения списка доступных семплеров
        /// </summary>
        public const string Samplers = $"{ApiPrefix}/samplers";

        /// <summary>
        /// Эндпоинт для получения списка доступных планировщиков (schedulers)
        /// </summary>
        public const string Schedulers = $"{ApiPrefix}/schedulers";
    }
}
