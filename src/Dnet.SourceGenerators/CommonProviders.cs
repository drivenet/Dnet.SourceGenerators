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

        var provider = context.SyntaxProvider.CreateSyntaxProvider((node, _) => nodePredicate(node), targetFactory);
        return Build(provider, comparer);
    }

    /// <summary>
    ///    Creates a provider that targets a syntax node, filtered by attribute name.
    /// </summary>
    ///
    /// <typeparam name="TTarget">The generator target type.</typeparam>
    /// <typeparam name="TComparer">The generator target comparer type.</typeparam>
    ///
    /// <param name="context">The generator initialization context.</param>
    /// <param name="fullyQualifiedMetadataName">A fully-qualified, metadata name of the attribute, including <c>Attribute</c> suffix.
    /// For example <c>System.CLSCompliantAttribute</c> for <see cref="CLSCompliantAttribute"/>.</param>
    /// <param name="nodePredicate">The syntax node predicate.</param>
    /// <param name="targetFactory">The generator target factory.</param>
    /// <param name="comparer">The generator target comparer.</param>
    ///
    /// <returns>An incremental values provider that provides generator targets.</returns>
    public static IncrementalValuesProvider<TTarget> TargetsWithAttribute<TTarget, TComparer>(
        IncrementalGeneratorInitializationContext context,
        string fullyQualifiedMetadataName,
        Predicate<SyntaxNode> nodePredicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, TTarget?> targetFactory,
        TComparer comparer)
        where TTarget : class
        where TComparer : class, IEqualityComparer<TTarget>
    {
        if (nodePredicate is null)
        {
            throw new ArgumentNullException(nameof(nodePredicate));
        }

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(fullyQualifiedMetadataName, (node, _) => nodePredicate(node), targetFactory);
        return Build(provider, comparer);
    }

    private static IncrementalValuesProvider<TTarget> Build<TTarget, TComparer>(IncrementalValuesProvider<TTarget?> provider, TComparer comparer)
        where TTarget : class
        where TComparer : class, IEqualityComparer<TTarget>
        => provider
            .Where(target => target is not null)!
            .WithComparer(comparer)
            .Collect()
            .SelectMany((targets, _) => targets.Distinct(comparer));
}
