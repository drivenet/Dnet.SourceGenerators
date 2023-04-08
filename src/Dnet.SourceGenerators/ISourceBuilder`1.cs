using System.Collections.Generic;
using System.Threading;

namespace Dnet.SourceGenerators;

/// <summary>
///     Source code builder.
/// </summary>
///
/// <typeparam name="TTarget">The target type.</typeparam>
public interface ISourceBuilder<TTarget>
    where TTarget : class
{
    /// <summary>
    ///    Builds the source code for the specified target.
    /// </summary>
    ///
    /// <param name="target">The target to build source code for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    ///
    /// <returns>The build results.</returns>
    IEnumerable<BuildResult> Build(TTarget target, CancellationToken cancellationToken);
}
