using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators;

/// <summary>
///     Recommended <see cref="SymbolDisplayFormat"/>s.
/// </summary>
public static class DisplayFormats
{
    /// <summary>
    ///     A format that includes the full namespace and type name, including the global namespace (naming any type).
    /// </summary>
    public static readonly SymbolDisplayFormat Full = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>
    ///     A format that is recommended for assembly-referenced type names (naming the same-assembly type).
    /// </summary>
    public static readonly SymbolDisplayFormat Local = Full
        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    /// <summary>
    ///     A format that is recommended for self-referenced type names (naming the generated type).
    /// </summary>
    public static readonly SymbolDisplayFormat Self = SymbolDisplayFormat.MinimallyQualifiedFormat
        .WithGenericsOptions(SymbolDisplayGenericsOptions.None);
}
