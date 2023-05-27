using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators.Examples;

[Generator]
internal sealed class ConstructorGenerator : CompositeGeneratorBase
{
    private const string AttributeTypeName = "System.GenerateConstructorAttribute";

    private const string AttributeSource = @"namespace System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class GenerateConstructorAttribute : Attribute
{
}
";

    private static readonly SourceInfo AttributeSourceInfo = new(AttributeTypeName, AttributeSource);

    protected override IEnumerable<IIncrementalGenerator> Inner
    {
        get
        {
            yield return CommonGenerators.SymbolTargetedWithAttribute<TypeTarget<ClassDeclarationSyntax>, ConstructorBuilder>(AttributeTypeName, NodePredicate, TargetFactory);
            yield return CommonGenerators.ExtraSources(AttributeSourceInfo);
        }
    }

    internal static bool IsValid(INamedTypeSymbol type)
        => type.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString(NullableFlowState.None, DisplayFormats.Local) == AttributeTypeName);

    private static bool NodePredicate(SyntaxNode node)
        => node is ClassDeclarationSyntax;

    private static TypeTarget<ClassDeclarationSyntax>? TargetFactory(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        => new((ClassDeclarationSyntax)context.TargetNode, (INamedTypeSymbol)context.TargetSymbol);
}
