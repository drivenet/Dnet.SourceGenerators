using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators;

/// <summary>
///     Common incremental generator providers.
/// </summary>
public static class CommonProviders
{
    /// <summary>
    ///    Creates a provider that caches an arbitrary item.
    /// </summary>
    ///
    /// <typeparam name="TItem">The type of the cached item.</typeparam>
    ///
    /// <param name="context">The generator initialization context.</param>
    ///
    /// <returns>An incremental value provider that provides a cached item.</returns>
    public static IncrementalValueProvider<TItem> CompilationCached<TItem>(IncrementalGeneratorInitializationContext context)
        where TItem : class, new()
        => context.CompilationProvider.Select((_, _) => new TItem());

    /// <summary>
    ///    Creates a provider that targets a syntax node.
    /// </summary>
    ///
    /// <typeparam name="TTarget">The generator target type.</typeparam>
    /// <typeparam name="TComparer">The generator target comparer type.</typeparam>
    ///
    /// <param name="context">The generator initialization context.</param>
    /// <param name="nodePredicate">The syntax node predicate.</param>
    /// <param name="targetFactory">The generator target factory.</param>
    /// <param name="comparer">The generator target comparer.</param>
    ///
    /// <returns>An incremental values provider that provides generator targets.</returns>
    public static IncrementalValuesProvider<TTarget> Targets<TTarget, TComparer>(
        IncrementalGeneratorInitializationContext context,
        Predicate<SyntaxNode> nodePredicate,
        Func<GeneratorSyntaxContext, CancellationToken, TTarget?> targetFactory,
        TComparer comparer)
        where TTarget : class
        where TComparer : class, IEqualityComparer<TTarget>
    {
        if (nodePredicate is null)
        {
            throw new ArgumentNullException(nameof(nodePredicate));
        }

        return context.SyntaxProvider.CreateSyntaxProvider((node, _) => nodePredicate(node), targetFactory)
            .Where(target => target is not null)!
            .WithComparer(comparer)
            .Collect()
            .SelectMany((targets, _) => targets.Distinct(comparer));
    }
}
