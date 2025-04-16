using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators;

/// <summary>
///     Common incremental outputs.
/// </summary>
public static class CommonOutputs
{
    public static void BuildSymbolTargeted<TTarget, TBuilder>(SourceProductionContext context, TTarget target)
        where TTarget : class, ISyntaxGeneratorTarget, ISymbolGeneratorTarget
        where TBuilder : class, ISourceBuilder<TTarget>, new()
        => GeneratorTools.RobustExecute(
            context,
            context => BuildSymbolTargeted(context, new TBuilder(), target),
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
