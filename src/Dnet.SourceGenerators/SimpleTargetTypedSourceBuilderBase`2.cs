using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     Represents a base for a source builder that targets a type with a single source.
/// </summary>
///
/// <typeparam name="TTarget">The target type.</typeparam>
/// <typeparam name="TDeclaration">The declaration type.</typeparam>
public abstract class SimpleTargetTypedSourceBuilderBase<TTarget, TDeclaration> : TargetTypedSourceBuilderBase<TTarget, TDeclaration>
    where TTarget : DeclaredSymbolTarget<TDeclaration, INamedTypeSymbol>
    where TDeclaration : TypeDeclarationSyntax
{
    /// <summary>
    ///     Initializes an instance of <see cref="SimpleTargetTypedSourceBuilderBase{TTarget, TDeclaration}"/> with default settings.
    /// </summary>
    protected SimpleTargetTypedSourceBuilderBase()
        : this(true)
    {
    }

    /// <summary>
    ///     Initializes an instance of <see cref="SimpleTargetTypedSourceBuilderBase{TTarget, TDeclaration}"/>.
    /// </summary>
    ///
    /// <param name="checkExtensibility"><c>true</c> to check for type's extensibility; elseway <c>false</c>.</param>
    protected SimpleTargetTypedSourceBuilderBase(bool checkExtensibility)
        : base(checkExtensibility)
    {
    }

    /// <inheritdoc/>
    protected sealed override IEnumerable<BuildResult> Build(TTarget target, string accessibility, CancellationToken cancellationToken)
    {
        yield return BuildSource(target, accessibility, cancellationToken);
    }

    /// <summary>
    ///     Builds the source for the specified target.
    /// </summary>
    ///
    /// <param name="target">The target to build source code for.</param>
    /// <param name="accessibility">The type accessibility.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    ///
    /// <returns>The build result.</returns>
    protected abstract BuildResult BuildSource(TTarget target, string accessibility, CancellationToken cancellationToken);
}
