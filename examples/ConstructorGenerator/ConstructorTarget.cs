using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators.Examples;

internal sealed record ConstructorTarget(
    ClassDeclarationSyntax Declaration,
    INamedTypeSymbol Type)
    : TypeTarget<ClassDeclarationSyntax>(Declaration, Type);
