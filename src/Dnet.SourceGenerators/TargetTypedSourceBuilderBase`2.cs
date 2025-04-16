using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     Represents a base for a source builder that targets a type.
/// </summary>
///
/// <typeparam name="TTarget">The target type.</typeparam>
/// <typeparam name="TDeclaration">The declaration type.</typeparam>
public abstract class TargetTypedSourceBuilderBase<TTarget, TDeclaration> : SymbolTargetedSourceBuilderBase<TTarget, TDeclaration, INamedTypeSymbol>
    where TTarget : DeclaredSymbolTarget<TDeclaration, INamedTypeSymbol>
    where TDeclaration : TypeDeclarationSyntax
{
    private readonly bool _extendType;

    /// <summary>
    ///     Initializes an instance of <see cref="TargetTypedSourceBuilderBase{TTarget, TDeclaration}"/> with default settings.
    /// </summary>
    protected TargetTypedSourceBuilderBase()
    {
    }

    /// <summary>
    ///     Initializes an instance of <see cref="TargetTypedSourceBuilderBase{TTarget, TDeclaration}"/>.
    /// </summary>
    ///
    /// <param name="extendType"><c>true</c> to check for type's extensibility; elseway <c>false</c>.</param>
    protected TargetTypedSourceBuilderBase(bool extendType)
    {
        _extendType = extendType;
    }

    protected override IEnumerable<Diagnostic> Verify(TTarget target)
    {
        if (_extendType
            && !GeneratorTools.TryExtend(target, out var error))
        {
            yield return error;
        }
    }
}
