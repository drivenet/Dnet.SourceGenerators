using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators.Examples;

internal readonly record struct ConstructorParameter(
    ITypeSymbol Type,
    string Name);
