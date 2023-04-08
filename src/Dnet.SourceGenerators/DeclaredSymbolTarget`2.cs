using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dnet.SourceGenerators;

/// <summary>
///     A base record for a generator target that combines a declaration syntax node and a symbol.
/// </summary>
///
/// <typeparam name="TDeclaration">The declaration syntax node type.</typeparam>
/// <typeparam name="TSymbol">The declared symbol type.</typeparam>
///
/// <param name="Declaration">The declaration syntax node.</param>
/// <param name="Type">The declared symbol.</param>
public abstract record DeclaredSymbolTarget<TDeclaration, TSymbol>(
    TDeclaration Declaration,
    TSymbol Type)
    : ISyntaxGeneratorTarget, ISymbolGeneratorTarget
    where TDeclaration : CSharpSyntaxNode
    where TSymbol : class, ISymbol
{
    /// <summary>
    ///     The declaration syntax node.
    /// </summary>
    public TDeclaration Declaration { get; } = Declaration ?? throw new ArgumentNullException(nameof(Declaration));

    /// <summary>
    ///     The declared symbol.
    /// </summary>
    public TSymbol Type { get; } = Type ?? throw new ArgumentNullException(nameof(Type));

    /// <inheritdoc/>
#pragma warning disable CA1033 // Interface methods should be callable by child types -- Type is more suitable
    CSharpSyntaxNode ISyntaxGeneratorTarget.SyntaxNode => Declaration;
#pragma warning restore CA1033 // Interface methods should be callable by child types

    /// <inheritdoc/>
#pragma warning disable CA1033 // Interface methods should be callable by child types -- Type is more suitable
    ISymbol ISymbolGeneratorTarget.Symbol => Type;
#pragma warning restore CA1033 // Interface methods should be callable by child types
}
