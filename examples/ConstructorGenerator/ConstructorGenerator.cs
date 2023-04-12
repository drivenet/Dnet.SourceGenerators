using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dnet.SourceGenerators.Examples;

[Generator]
internal sealed class ConstructorGenerator : CompositeGeneratorBase
{
    private const string AttributeShortName = "GenerateConstructor";
    private const string AttributeFullName = AttributeShortName + "Attribute";
    private const string AttributeTypeName = "System." + AttributeFullName;

    private const string AttributeSource = @"namespace System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class GenerateConstructorAttribute : Attribute
{
}
";

    private static readonly SourceInfo AttributeSourceInfo = new(AttributeFullName, SourceText.From(AttributeSource, Encoding.UTF8));

    protected override IEnumerable<IIncrementalGenerator> Inner
    {
        get
        {
            yield return CommonGenerators.SymbolTargeted<TypeTarget, ConstructorBuilder>(NodePredicate, TargetFactory);
            yield return CommonGenerators.ExtraSources(AttributeSourceInfo);
        }
    }

    internal static bool IsValid(INamedTypeSymbol type)
        => type.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString(NullableFlowState.None, DisplayFormats.Local) == AttributeTypeName)
            is { } attributeData;

    private static bool NodePredicate(SyntaxNode node)
        => node is AttributeSyntax attribute
            && GeneratorTools.GetNameText(attribute.Name) is AttributeShortName or AttributeFullName
            && !GeneratorTools.ContainsErrors(attribute)
            && attribute.Parent?.Parent is ClassDeclarationSyntax;

    private static TypeTarget? TargetFactory(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var declaration = (ClassDeclarationSyntax)context.Node.Parent!.Parent!;
        return context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken) is INamedTypeSymbol type
            && IsValid(type)
            ? new(declaration, type)
            : null;
    }
}
