using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Dnet.SourceGenerators.Core;

using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators;

/// <summary>
///     Common incremental generators.
/// </summary>
public static class CommonGenerators
{
    /// <summary>
    ///    Creates a generator that adds extra sources to the compilation.
    /// </summary>
    ///
    /// <param name="sources">The extra sources to add.</param>
    ///
    /// <returns>An implementation of a generator that adds extra sources to the compilation.</returns>
#pragma warning disable CA1859 // Future-proofing
    public static IIncrementalGenerator ExtraSources(IEnumerable<SourceInfo> sources)
#pragma warning restore CA1859
    {
        if (sources is null)
        {
            throw new ArgumentNullException(nameof(sources));
        }

        return new ActionAdaptingGenerator(context => context.RegisterPostInitializationOutput(postInitContext => AddSources(postInitContext, sources)));
    }

    /// <summary>
    ///    Creates a generator that adds extra sources to the compilation.
    /// </summary>
    ///
    /// <param name="sources">The extra sources to add.</param>
    ///
    /// <returns>An implementation of a generator that adds extra sources to the compilation.</returns>
    public static IIncrementalGenerator ExtraSources(params SourceInfo[] sources)
        => ExtraSources(sources.AsEnumerable());

    /// <summary>
    ///     Creates a generator that targets a syntax node.
    /// </summary>
    ///
    /// <typeparam name="TTarget">The generator target type.</typeparam>
    /// <typeparam name="TBuilder">The generator builder type.</typeparam>
    ///
    /// <param name="nodePredicate">The syntax node predicate.</param>
    /// <param name="targetFactory">The generator target factor.</param>
    ///
    /// <returns>An implementation of a generator that builds source from specific syntax nodes.</returns>
#pragma warning disable CA1859 // Future-proofing
    public static IIncrementalGenerator SymbolTargeted<TTarget, TBuilder>(
        Predicate<SyntaxNode> nodePredicate,
        Func<GeneratorSyntaxContext, CancellationToken, TTarget?> targetFactory)
#pragma warning restore CA1859
        where TTarget : class, ISyntaxGeneratorTarget, ISymbolGeneratorTarget
        where TBuilder : class, ISourceBuilder<TTarget>, new()
    {
        if (nodePredicate is null)
        {
            throw new ArgumentNullException(nameof(nodePredicate));
        }

        if (targetFactory is null)
        {
            throw new ArgumentNullException(nameof(targetFactory));
        }

        return new ActionAdaptingGenerator(context =>
        {
            var pipeline = CommonProviders.Targets(context, nodePredicate, targetFactory, SymbolGeneratorTargetComparer.Instance)
                .Combine(CommonProviders.CompilationCached<TBuilder>(context))
                .WithComparer(SyntaxGeneratorTargetPipelineComparer<TTarget, TBuilder>.Instance);
            context.RegisterSourceOutput(pipeline, Build);
        });
    }

    /// <summary>
    ///     Creates a generator that targets a syntax node marked with an attribute with the specified name.
    /// </summary>
    ///
    /// <typeparam name="TTarget">The generator target type.</typeparam>
    /// <typeparam name="TBuilder">The generator builder type.</typeparam>
    ///
    /// <param name="fullyQualifiedMetadataName">A fully-qualified, metadata name of the attribute, including <c>Attribute</c> suffix.
    /// For example <c>"System.CLSCompliantAttribute</c> for <see cref="System.CLSCompliantAttribute"/>.</param>
    /// <param name="nodePredicate">The syntax node predicate.</param>
    /// <param name="targetFactory">The generator target factor.</param>
    ///
    /// <returns>An implementation of a generator that builds source from specific syntax nodes.</returns>
#pragma warning disable CA1859 // Future-proofing
    public static IIncrementalGenerator SymbolTargetedWithAttribute<TTarget, TBuilder>(
        string fullyQualifiedMetadataName,
        Predicate<SyntaxNode> nodePredicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, TTarget?> targetFactory)
#pragma warning restore CA1859
        where TTarget : class, ISyntaxGeneratorTarget, ISymbolGeneratorTarget
        where TBuilder : class, ISourceBuilder<TTarget>, new()
    {
        if (nodePredicate is null)
        {
            throw new ArgumentNullException(nameof(nodePredicate));
        }

        if (targetFactory is null)
        {
            throw new ArgumentNullException(nameof(targetFactory));
        }

        return new ActionAdaptingGenerator(context =>
        {
            var pipeline = CommonProviders.TargetsWithAttribute(context, fullyQualifiedMetadataName, nodePredicate, targetFactory, SymbolGeneratorTargetComparer.Instance)
                .Combine(CommonProviders.CompilationCached<TBuilder>(context))
                .WithComparer(SyntaxGeneratorTargetPipelineComparer<TTarget, TBuilder>.Instance);
            context.RegisterSourceOutput(pipeline, Build);
        });
    }

    private static void AddSources(IncrementalGeneratorPostInitializationContext context, IEnumerable<SourceInfo> sources)
    {
        foreach (var result in sources)
        {
            context.AddSource(result.Name + ".g", "// <auto-generated/>" + Environment.NewLine + result.Text);
        }
    }

    private static void Build<TTarget, TBuilder>(SourceProductionContext context, (TTarget Target, TBuilder Builder) source)
        where TTarget : class, ISyntaxGeneratorTarget, ISymbolGeneratorTarget
        where TBuilder : class, ISourceBuilder<TTarget>
        => GeneratorTools.RobustExecute(
            context,
            context => BuildSymbolTargeted(context, source.Builder, source.Target),
            exception => GeneratorTools.CreateExceptionDiagnostic(exception, null));

    private static void BuildSymbolTargeted<TTarget, TBuilder>(SourceProductionContext context, TBuilder builder, TTarget target)
        where TTarget : class, ISyntaxGeneratorTarget, ISymbolGeneratorTarget
        where TBuilder : class, ISourceBuilder<TTarget>
    {
        var results = GeneratorTools.RobustExecute(
            context,
            context => builder.Build(target, context.CancellationToken),
            exception => GeneratorTools.CreateExceptionDiagnostic(exception, target.SyntaxNode.GetLocation(), target),
            Enumerable.Empty<BuildResult>());
        var symbol = target.Symbol;
        var prefix = symbol.ContainingNamespace is { IsGlobalNamespace: true } ? "global_" : (symbol.ContainingNamespace?.ToString() ?? "_");
        var defaultName = prefix + "." + symbol.MetadataName.Replace('`', '_');
        AddResults(context, results, defaultName);
    }

    private static void AddResults(SourceProductionContext context, IEnumerable<BuildResult> results, string defaultName)
    {
        Action<Diagnostic> reportDiagnostic = context.ReportDiagnostic;
        foreach (var result in results)
        {
            result.Apply(
                (name, text) =>
                {
                    var hintName = defaultName;
                    if (!string.IsNullOrEmpty(name))
                    {
                        hintName = hintName + "." + name;
                    }

                    context.AddSource(hintName + ".g", text);
                },
                reportDiagnostic);
        }
    }
}
