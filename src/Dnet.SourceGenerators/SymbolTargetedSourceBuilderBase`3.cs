﻿using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     Represents a base for a source builder that targets a symbol.
/// </summary>
///
/// <typeparam name="TTarget">The target symbol.</typeparam>
/// <typeparam name="TDeclaration">The declaration syntax node type.</typeparam>
/// <typeparam name="TSymbol">The declared symbol type.</typeparam>
public abstract class SymbolTargetedSourceBuilderBase<TTarget, TDeclaration, TSymbol> : ISourceBuilder<TTarget>
    where TTarget : DeclaredSymbolTarget<TDeclaration, TSymbol>
    where TDeclaration : MemberDeclarationSyntax
    where TSymbol : class, ISymbol
{
    /// <summary>
    ///     The string builder used to build the source.
    /// </summary>
    protected StringBuilder Builder { get; } = new();

    /// <inheritdoc/>
    public IEnumerable<BuildResult> Build(TTarget target, CancellationToken cancellationToken)
    {
        var type = target.Symbol as ITypeSymbol ?? target.Symbol.ContainingType;
        var accessibility = GeneratorTools.GetTopLevelAccessibility(type);
        if (accessibility is null)
        {
            yield return new(Diagnostic.Create(GeneratorDiagnostics.InvalidTopLevelTypeAccessibility, target.Declaration.GetLocation(), type));
        }

        foreach (var diagnostic in Verify(target))
        {
            yield return new(diagnostic);
            accessibility = null;
        }

        if (accessibility is null)
        {
            yield break;
        }

        Builder.Clear();
        Builder.AppendLine("// <auto-generated/>");
        foreach (var result in Build(target, accessibility, cancellationToken))
        {
            yield return result;
            Builder.Clear();
        }
    }

    protected abstract IEnumerable<Diagnostic> Verify(TTarget target);

    /// <summary>
    ///     Builds the source for the specified target.
    /// </summary>
    ///
    /// <param name="target">The target to build source code for.</param>
    /// <param name="accessibility">The type accessibility.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    ///
    /// <returns>The build results.</returns>
    protected abstract IEnumerable<BuildResult> Build(TTarget target, string accessibility, CancellationToken cancellationToken);
}
