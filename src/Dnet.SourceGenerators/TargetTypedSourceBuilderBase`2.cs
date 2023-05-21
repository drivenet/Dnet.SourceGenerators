using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     Represents a base for a source builder that extends a type.
/// </summary>
///
/// <typeparam name="TTarget">The target type.</typeparam>
/// <typeparam name="TDeclaration">The declaration type.</typeparam>
public abstract class TargetTypedSourceBuilderBase<TTarget, TDeclaration> : ISourceBuilder<TTarget>
    where TTarget : DeclaredSymbolTarget<TDeclaration, INamedTypeSymbol>
    where TDeclaration : TypeDeclarationSyntax
{
    /// <summary>
    ///     The string builder used to build the source.
    /// </summary>
    protected StringBuilder Builder { get; } = new();

    /// <inheritdoc/>
    public IEnumerable<BuildResult> Build(TTarget target, CancellationToken cancellationToken)
    {
        if (!GeneratorTools.TryExtend(target, out var accessibility, out var error))
        {
            yield return new(error);
            yield break;
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
