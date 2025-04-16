using System.Collections.Generic;
using System.Threading;

namespace Dnet.SourceGenerators;

/// <summary>
///     Represents a base for a source builder that targets a method with a single source.
/// </summary>
///
/// <typeparam name="TTarget">The target type.</typeparam>
public abstract class SimpleMethodTargetedBuilderBase<TTarget> : MethodTargetedSourceBuilderBase<TTarget>
    where TTarget : MethodTarget
{
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
