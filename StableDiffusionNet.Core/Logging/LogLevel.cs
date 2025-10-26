namespace StableDiffusionNet.Logging
{
    /// <summary>
    /// Уровни логирования
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Отладочная информация
        /// </summary>
        Debug = 0,

        /// <summary>
        /// Информационные сообщения
        /// </summary>
        Information = 1,

        /// <summary>
        /// Предупреждения
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Ошибки
        /// </summary>
        Error = 3,

        /// <summary>
        /// Критические ошибки
        /// </summary>
        Critical = 4,
    }
}
