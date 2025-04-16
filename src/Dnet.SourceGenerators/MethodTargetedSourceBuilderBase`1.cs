using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     Represents a base for a source builder that targets a method.
/// </summary>
///
/// <typeparam name="TTarget">The target symbol.</typeparam>
public abstract class MethodTargetedSourceBuilderBase<TTarget> : SymbolTargetedSourceBuilderBase<TTarget, MethodDeclarationSyntax, IMethodSymbol>
    where TTarget : MethodTarget
{
    protected override IEnumerable<Diagnostic> Verify(TTarget target)
    {
        if (!GeneratorTools.TryExtend(target, out var error))
        {
            yield return error;
        }
    }
}
