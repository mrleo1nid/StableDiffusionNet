#if NETSTANDARD2_0 || NETSTANDARD2_1
// Полифилл для поддержки init accessors в .NET Standard 2.0/2.1
// Этот класс требуется для компиляции record и init-only свойств в старых версиях .NET

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Служебный класс для поддержки init-only свойств в .NET Standard 2.0/2.1.
    /// ВАЖНО: Класс должен быть пустым - это маркер-тип для компилятора C#.
    /// </summary>
#pragma warning disable S2094 // Classes should not be empty
    internal static class IsExternalInit { }
#pragma warning restore S2094
}
#endif
