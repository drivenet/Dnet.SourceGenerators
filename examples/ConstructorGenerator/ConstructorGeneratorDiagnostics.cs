using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators.Examples;

internal static class ConstructorGeneratorDiagnostics
{
    public static readonly DiagnosticDescriptor MultiplePreferredConstructors =
        new(
            "DN7206",
            "Multiple preferred constructors",
            "The type '{0}' must have only one preferred (max-arity) constructor",
            GeneratorDiagnostics.DiagnosticCategory,
            DiagnosticSeverity.Error,
            true);
}
