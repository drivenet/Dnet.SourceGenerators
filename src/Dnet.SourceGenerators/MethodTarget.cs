using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     A generator target for any method declaration.
/// </summary>
///
/// <param name="Declaration">The method declaration.</param>
/// <param name="Method">The method symbol.</param>
public record MethodTarget(
    MethodDeclarationSyntax Declaration,
    IMethodSymbol Method)
    : DeclaredSymbolTarget<MethodDeclarationSyntax, IMethodSymbol>(Declaration, Method)
{
}
