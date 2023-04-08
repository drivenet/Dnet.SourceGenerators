using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators;

/// <summary>
///     A base class for composite generators.
/// </summary>
public abstract class CompositeGeneratorBase : IIncrementalGenerator
{
    /// <summary>
    ///     Gets the inner generators.
    /// </summary>
    protected abstract IEnumerable<IIncrementalGenerator> Inner { get; }

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var generator in Inner)
        {
            generator.Initialize(context);
        }
    }
}
