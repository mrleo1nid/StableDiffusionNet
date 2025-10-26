using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Вспомогательный класс для проверки аргументов на null
    /// Обеспечивает совместимость с .NET Standard 2.0/2.1 и .NET 6.0+
    /// </summary>
    internal static class Guard
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// Выбрасывает ArgumentNullException если аргумент null
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
        }
#else
        /// <summary>
        /// Выбрасывает ArgumentNullException если аргумент null
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull(object? argument, string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
#endif
    }
}
