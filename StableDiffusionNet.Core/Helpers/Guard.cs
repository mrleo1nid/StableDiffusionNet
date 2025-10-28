using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StableDiffusionNet.Helpers
{
    /// <summary>
    /// Вспомогательный класс для валидации аргументов.
    /// Централизует проверки и уменьшает дублирование кода.
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

        /// <summary>
        /// Выбрасывает ArgumentException если строка null или пустая
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullOrEmpty(
#if NET6_0_OR_GREATER
            [NotNull]
#endif
            string? argument,
#if NET6_0_OR_GREATER
            [CallerArgumentExpression(nameof(argument))]
#endif
            string? paramName = null
        )
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("Value cannot be null or empty", paramName);
            }
        }

        /// <summary>
        /// Выбрасывает ArgumentException если строка null, пустая или содержит только пробелы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullOrWhiteSpace(
#if NET6_0_OR_GREATER
            [NotNull]
#endif
            string? argument,
#if NET6_0_OR_GREATER
            [CallerArgumentExpression(nameof(argument))]
#endif
            string? paramName = null
        )
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException(
                    "Value cannot be null, empty, or whitespace",
                    paramName
                );
            }
        }

        /// <summary>
        /// Выбрасывает ArgumentException если строка null, пустая или содержит только пробелы.
        /// Эта перегрузка принимает явное имя параметра и не использует CallerArgumentExpression.
        /// Используется для вспомогательных методов валидации где нужно сохранить оригинальное имя параметра.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullOrWhiteSpaceWithName(
#if NET6_0_OR_GREATER
            [NotNull]
#endif
            string? argument,
            string paramName
        )
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException(
                    "Value cannot be null, empty, or whitespace",
                    paramName
                );
            }
        }

        /// <summary>
        /// Выбрасывает ArgumentOutOfRangeException если значение вне допустимого диапазона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfOutOfRange<T>(
            T argument,
            T min,
            T max,
#if NET6_0_OR_GREATER
            [CallerArgumentExpression(nameof(argument))]
#endif
            string? paramName = null
        )
            where T : IComparable<T>
        {
            if (argument.CompareTo(min) < 0 || argument.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    argument,
                    $"Value must be between {min} and {max}"
                );
            }
        }

        /// <summary>
        /// Выбрасывает ArgumentOutOfRangeException если значение отрицательное или ноль
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNegativeOrZero(
            int argument,
#if NET6_0_OR_GREATER
            [CallerArgumentExpression(nameof(argument))]
#endif
            string? paramName = null
        )
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    argument,
                    "Value must be greater than zero"
                );
            }
        }

        /// <summary>
        /// Выбрасывает ArgumentOutOfRangeException если значение отрицательное или ноль (long)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNegativeOrZero(
            long argument,
#if NET6_0_OR_GREATER
            [CallerArgumentExpression(nameof(argument))]
#endif
            string? paramName = null
        )
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    argument,
                    "Value must be greater than zero"
                );
            }
        }

        /// <summary>
        /// Выбрасывает ArgumentException если коллекция null или пустая
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfEmpty<T>(
#if NET6_0_OR_GREATER
            [NotNull]
#endif
            IEnumerable<T>? argument,
#if NET6_0_OR_GREATER
            [CallerArgumentExpression(nameof(argument))]
#endif
            string? paramName = null
        )
        {
            if (argument == null || !argument.Any())
            {
                throw new ArgumentException("Collection cannot be null or empty", paramName);
            }
        }
    }
}
