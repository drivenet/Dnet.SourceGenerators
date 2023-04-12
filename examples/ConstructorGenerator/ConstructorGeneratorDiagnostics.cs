using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators.Examples;

internal static class ConstructorGeneratorDiagnostics
{
    public static readonly DiagnosticDescriptor ValueTypesAreNotSupported =
        new(
            "DN7204",
            "Value types are not supported",
            "Change the type '{0}' to class",
            GeneratorDiagnostics.DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);

    public static readonly DiagnosticDescriptor MultiplePreferredConstructors =
        new(
            "DN7206",
            "Multiple preferred constructors",
            "The type '{0}' must have only one preferred (max-arity) constructor",
            GeneratorDiagnostics.DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);
}
