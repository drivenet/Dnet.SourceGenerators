﻿using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     Represents a base for a source builder that targets a type.
/// </summary>
///
/// <typeparam name="TTarget">The target type.</typeparam>
/// <typeparam name="TDeclaration">The declaration type.</typeparam>
public abstract class TargetTypedSourceBuilderBase<TTarget, TDeclaration> : ISourceBuilder<TTarget>
    where TTarget : DeclaredSymbolTarget<TDeclaration, INamedTypeSymbol>
    where TDeclaration : TypeDeclarationSyntax
{
    private readonly bool _extendType;

    /// <summary>
    ///     Initializes an instance of <see cref="TargetTypedSourceBuilderBase{TTarget, TDeclaration}"/> with default settings.
    /// </summary>
    protected TargetTypedSourceBuilderBase()
        : this(true)
    {
    }

    /// <summary>
    ///     Initializes an instance of <see cref="TargetTypedSourceBuilderBase{TTarget, TDeclaration}"/>.
    /// </summary>
    ///
    /// <param name="extendType"><c>true</c> to check for type's extensibility; elseway <c>false</c>.</param>
    protected TargetTypedSourceBuilderBase(bool extendType)
    {
        _extendType = extendType;
    }

    /// <summary>
    ///     The string builder used to build the source.
    /// </summary>
    protected StringBuilder Builder { get; } = new();

    /// <inheritdoc/>
    public IEnumerable<BuildResult> Build(TTarget target, CancellationToken cancellationToken)
    {
        string? accessibility;
        if (_extendType)
        {
            if (!GeneratorTools.TryExtend(target, out accessibility, out var error))
            {
                yield return new(error);
                yield break;
            }
        }
        else
        {
            var type = target.Type;
            accessibility = GeneratorTools.GetTopLevelAccessibility(type);
            if (accessibility is null)
            {
                yield return new(Diagnostic.Create(GeneratorDiagnostics.InvalidTopLevelTypeAccessibility, target.Declaration.GetLocation(), type));
                yield break;
            }
        }

        Builder.Clear();
        foreach (var result in Build(target, accessibility, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return result;
            Builder.Clear();
        }
    }

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
