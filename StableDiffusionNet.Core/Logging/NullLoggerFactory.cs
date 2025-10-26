namespace StableDiffusionNet.Logging
{
    /// <summary>
    /// Фабрика, которая создает пустые логгеры
    /// </summary>
    public sealed class NullLoggerFactory : IStableDiffusionLoggerFactory
    {
        /// <summary>
        /// Единственный экземпляр NullLoggerFactory
        /// </summary>
        public static readonly NullLoggerFactory Instance = new NullLoggerFactory();

        private NullLoggerFactory() { }

        /// <inheritdoc/>
        public IStableDiffusionLogger CreateLogger<T>()
        {
            return NullLogger.Instance;
        }

        /// <inheritdoc/>
        public IStableDiffusionLogger CreateLogger(string categoryName)
        {
            return NullLogger.Instance;
        }
    }
}
