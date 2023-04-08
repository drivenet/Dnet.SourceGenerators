using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     A generator target for a specific kind of a type declaration.
/// </summary>
///
/// <typeparam name="TDeclaration">The type of the type declaration.</typeparam>
///
/// <param name="Declaration">The type declaration.</param>
/// <param name="Type">The type symbol.</param>
public record TypeTarget<TDeclaration>(
    TDeclaration Declaration,
    INamedTypeSymbol Type)
    : DeclaredSymbolTarget<TDeclaration, INamedTypeSymbol>(Declaration, Type)
    where TDeclaration : TypeDeclarationSyntax
{
}
