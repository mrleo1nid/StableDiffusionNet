namespace StableDiffusionNet.Logging
{
    /// <summary>
    /// Фабрика для создания логгеров
    /// </summary>
    public interface IStableDiffusionLoggerFactory
    {
        /// <summary>
        /// Создает логгер для указанного типа
        /// </summary>
        /// <typeparam name="T">Тип, для которого создается логгер</typeparam>
        /// <returns>Экземпляр логгера</returns>
        IStableDiffusionLogger CreateLogger<T>();

        /// <summary>
        /// Создает логгер с указанным именем категории
        /// </summary>
        /// <param name="categoryName">Имя категории</param>
        /// <returns>Экземпляр логгера</returns>
        IStableDiffusionLogger CreateLogger(string categoryName);
    }
}
