namespace StableDiffusionNet.Tests
{
    /// <summary>
    /// Категории тестов для фильтрации
    /// </summary>
    public static class TestCategories
    {
        /// <summary>
        /// Unit-тесты (быстрые, без внешних зависимостей)
        /// </summary>
        public const string Unit = "Unit";

        /// <summary>
        /// Интеграционные тесты (требуют запущенный API)
        /// </summary>
        public const string Integration = "Integration";

        /// <summary>
        /// Долгие тесты (генерация изображений и т.д.)
        /// </summary>
        public const string LongRunning = "LongRunning";

        /// <summary>
        /// Быстрые интеграционные тесты (проверка доступности API)
        /// </summary>
        public const string Smoke = "Smoke";
    }
}
