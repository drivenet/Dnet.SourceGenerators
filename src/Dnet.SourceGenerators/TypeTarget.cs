using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     A generator target for any type declaration.
/// </summary>
///
/// <param name="Declaration">The type declaration.</param>
/// <param name="Type">The type symbol.</param>
public record TypeTarget(
    TypeDeclarationSyntax Declaration,
    INamedTypeSymbol Type)
    : DeclaredSymbolTarget<TypeDeclarationSyntax, INamedTypeSymbol>(Declaration, Type)
{
}
