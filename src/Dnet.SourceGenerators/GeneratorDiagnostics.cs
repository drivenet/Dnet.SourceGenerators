using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators;

/// <summary>
///    A collection of diagnostic descriptors for source generators.
/// </summary>
public static class GeneratorDiagnostics
{
    /// <summary>
    ///    The diagnostic category for all diagnostics produced by this library.
    /// </summary>
    public const string DiagnosticCategory = "Dnet.SourceGenerator";

    /// <summary>
    ///     The diagnostic descriptor for unhandled exceptions.
    /// </summary>
    public static readonly DiagnosticDescriptor UnhandledException =
        new(
            "DN0001",
            "Unhandled exception occurred",
            "The generator caused an exception {0}: {1}",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    ///     The diagnostic descriptor for contextful unhandled exceptions.
    /// </summary>
    public static readonly DiagnosticDescriptor ContextfulUnhandledException =
        new(
            "DN0001",
            "Unhandled exception occurred",
            "While processing {0} the generator caused an exception {1}: {2}",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    ///     The diagnostic descriptor for top-level types being unsupported.
    /// </summary>
    public static readonly DiagnosticDescriptor TopLevelTypesAreNotSupported =
        new(
            "DN0002",
            "Top-level generated types are not supported",
            "Place the type '{0}' inside a namespace",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    ///     The diagnostic descriptor for missing partial keyword.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingPartialKeyword =
        new(
            "DN0003",
            "Additive generated code must be partial",
            "Add partial keyword to the '{0}'",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    ///     The diagnostic descriptor for contained types being unsupported.
    /// </summary>
    public static readonly DiagnosticDescriptor ContainedTypesAreNotSupported =
        new(
            "DN0004",
            "Additive type generation for contained types is not supported",
            "Move the type '{0}' out of the containing type",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    ///     The diagnostic descriptor for invalid top-level type accessibility.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidTopLevelTypeAccessibility =
        new(
            "DN0005",
            "Invalid top-level type accessibility",
            "Set the type '{0}' accessibility to 'public' or 'internal'",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    ///     The diagnostic descriptor for missing result or diagnostic.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingResultOrDiagnostic =
        new(
            "DN0006",
            "Missing result or diagnostic",
            "The generator did not return a result or a diagnostic",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    ///     The diagnostic descriptor for invalid member accessibility.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidMemberAccessibility =
        new(
            "DN0007",
            "Invalid member accessibility",
            "Set the type '{0}' accessibility",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);
}
