using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static System.FormattableString;

namespace Dnet.SourceGenerators.Examples;

internal sealed class ConstructorBuilder : SimpleTargetTypedSourceBuilderBase<TypeTarget<ClassDeclarationSyntax>, ClassDeclarationSyntax>
{
    private readonly Dictionary<INamedTypeSymbol, IReadOnlyCollection<IFieldSymbol>> _fieldCache = new(SymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<INamedTypeSymbol, IReadOnlyCollection<IPropertySymbol>> _propertyCache = new(SymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<INamedTypeSymbol, IReadOnlyList<IReadOnlyCollection<ConstructorParameter>>> _signaturesCache = new(SymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<INamedTypeSymbol, IReadOnlyList<IReadOnlyCollection<ConstructorParameter>>> _localSignaturesCache = new(SymbolEqualityComparer.IncludeNullability);

    protected override BuildResult BuildSource(TypeTarget<ClassDeclarationSyntax> target, string accessibility, CancellationToken cancellationToken)
    {
        var type = target.Type;
        var baseType = type.BaseType ?? throw new InvalidOperationException(Invariant($"Unexpected missing base type for reference type {type}."));
        var ctorSignatures = GetSignatures(baseType, cancellationToken);
        var baseParameters = ctorSignatures.Count != 0
            ? ctorSignatures[0]
            : Array.Empty<ConstructorParameter>();
        if (ctorSignatures.Count > 1
            && baseParameters.Count == ctorSignatures[1].Count)
        {
            var location = target.Declaration.GetLocation();
            return new(Diagnostic.Create(ConstructorGeneratorDiagnostics.MultiplePreferredConstructors, location, baseType));
        }

        var fields = GetFields(type, cancellationToken);
        var properties = GetProperties(type, cancellationToken);
        if (fields.Count == 0
            && properties.Count == 0
            && baseParameters.Count == 0)
        {
            return BuildResult.Empty;
        }

        return new(Build(type, accessibility, baseParameters, fields, properties));
    }

    private string Build(
        ITypeSymbol type,
        string accessibility,
        IReadOnlyCollection<ConstructorParameter> baseParameters,
        IReadOnlyCollection<IFieldSymbol> fields,
        IReadOnlyCollection<IPropertySymbol> properties)
    {
        var typeNamespace = type.ContainingNamespace.ToString();
        var typeName = type.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.MinimallyQualifiedFormat);
        EmitUsings();
        EmitPrologue(accessibility, typeNamespace, typeName);
        EmitCtor(type, baseParameters, fields, properties);
        EmitEpilogue();
        return Builder.ToString();
    }

    private void EmitUsings()
    {
        Builder.AppendLine("#nullable enable");
    }

    private void EmitPrologue(string accessibility, string typeNamespace, string typeName)
    {
        Builder.Append(Invariant($@"namespace {typeNamespace};

#pragma warning disable CS0618 // Required if depending types are obsolete

{accessibility} partial class {typeName}
{{"));
    }

    private void EmitCtor(
        ITypeSymbol type,
        IReadOnlyCollection<ConstructorParameter> baseParameters,
        IReadOnlyCollection<IFieldSymbol> fields,
        IReadOnlyCollection<IPropertySymbol> properties)
    {
        var selfTypeName = type.ToDisplayString(NullableFlowState.NotNull, DisplayFormats.Self);
        var accessibility = type.IsAbstract ? "protected" : "public";
        Builder.Append(Invariant(@$"
    {accessibility} {selfTypeName}("));
        var argsCount = baseParameters.Count + fields.Count + properties.Count;
        if (argsCount > 1)
        {
            Builder.AppendLine();
        }

        var isFirst = true;
        var baseArguments = baseParameters
            .Select(parameter => (parameter, parameter.Name))
            .ToList();
        foreach (var (parameter, name) in baseArguments)
        {
            var parameterTypeName = parameter.Type.ToDisplayString(DisplayFormats.Full);
            if (!isFirst)
            {
                Builder.AppendLine(",");
            }

            if (argsCount > 1)
            {
                Builder.Append("        ");
            }

            Builder.Append(Invariant($"{parameterTypeName} @{name}"));
            isFirst = false;
        }

        var fieldsData = fields.Select(
            field => (
                field.Type,
                field.Name,
                field.Name.Substring(1)));
        var propertiesData = properties.Select(
            property => (
                property.Type,
                property.Name,
                char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1)));
        var arguments = fieldsData.Concat(propertiesData).ToList();
        foreach (var (argumentType, _, name) in arguments)
        {
            var typeName = argumentType.ToDisplayString(DisplayFormats.Full);
            if (!isFirst)
            {
                Builder.AppendLine(",");
            }

            if (argsCount > 1)
            {
                Builder.Append("        ");
            }

            Builder.Append(Invariant($"{typeName} @{name}"));
            isFirst = false;
        }

        Builder.AppendLine(")");
        if (baseArguments.Count > 0)
        {
            Builder.Append("        : base(");
            if (baseArguments.Count > 1)
            {
                Builder.AppendLine();
            }

            isFirst = true;
            foreach (var (_, name) in baseArguments)
            {
                if (!isFirst)
                {
                    Builder.AppendLine(",");
                }

                if (argsCount > 1)
                {
                    Builder.Append("            ");
                }

                Builder.Append('@');
                Builder.Append(name);
                isFirst = false;
            }

            Builder.AppendLine(")");
        }

        Builder.AppendLine("    {");
        foreach (var (argumentType, originalName, name) in arguments)
        {
            Builder.Append(Invariant($"        {originalName} = @{name}"));
            if (GeneratorTools.IsNonNullableReferenceType(argumentType))
            {
                Builder.Append(Invariant($" ?? throw new global::System.ArgumentNullException(nameof(@{name}))"));
            }

            Builder.AppendLine(";");
        }

        Builder.AppendLine("    }");
    }

    private void EmitEpilogue()
    {
        Builder.Append(@"}
");
    }

    private IReadOnlyList<IReadOnlyCollection<ConstructorParameter>> GetSignatures(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        if (type.SpecialType == SpecialType.System_Object)
        {
            return Array.Empty<IReadOnlyCollection<ConstructorParameter>>();
        }

        var isLocalType = ConstructorGenerator.IsValid(type);
        var cache = isLocalType ? _localSignaturesCache : _signaturesCache;
        if (!cache.TryGetValue(type, out var list))
        {
            list = GetSignaturesCore(type, isLocalType, cancellationToken);
            cache.Add(type, list);
        }

        return list;
    }

    private IReadOnlyList<IReadOnlyCollection<ConstructorParameter>> GetSignaturesCore(INamedTypeSymbol type, bool isLocalType, CancellationToken cancellationToken)
    {
        var candidateSignatures = type.InstanceConstructors
            .Where(ctor => ctor.DeclaredAccessibility != Accessibility.Private)
            .Select(ctor => (IReadOnlyCollection<ConstructorParameter>)ctor.Parameters
                .Select(parameter => new ConstructorParameter(parameter.Type, parameter.Name))
                .ToList());
        if (isLocalType)
        {
            var baseFields = GetFields(type, cancellationToken);
            var baseSignatures = baseFields
                .Select(field => new ConstructorParameter(field.Type, field.Name.Substring(1)))
                .ToList();
            candidateSignatures = candidateSignatures.Concat(new[] { baseSignatures });
        }

        var sortedSignatures = candidateSignatures
            .OrderByDescending(parameters => parameters.Count)
            .Take(2)
            .ToList();
        return sortedSignatures;
    }

    private IReadOnlyCollection<IFieldSymbol> GetFields(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        if (type.IsSealed)
        {
            return GetFieldsCore(type, cancellationToken);
        }

        if (!_fieldCache.TryGetValue(type, out var fields))
        {
            fields = GetFieldsCore(type, cancellationToken);
            _fieldCache.Add(type, fields);
        }

        return fields;
    }

    private IReadOnlyCollection<IPropertySymbol> GetProperties(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        if (type.IsSealed)
        {
            return GetPropertiesCore(type, cancellationToken);
        }

        if (!_propertyCache.TryGetValue(type, out var properties))
        {
            properties = GetPropertiesCore(type, cancellationToken);
            _propertyCache.Add(type, properties);
        }

        return properties;
    }

    private static IReadOnlyCollection<IFieldSymbol> GetFieldsCore(INamedTypeSymbol type, CancellationToken cancellationToken)
        => type.GetMembers()
            .Where(member =>
                member.Kind == SymbolKind.Field
                && !member.IsStatic
                && member.IsDefinition
                && !member.IsImplicitlyDeclared
                && member.Name.StartsWith("_", StringComparison.Ordinal))
            .Cast<IFieldSymbol>()
            .Where(field => !field.IsConst
                && (field.IsReadOnly
                    || GeneratorTools.IsNonNullableReferenceType(field.Type))
                && field.DeclaringSyntaxReferences is { Length: 1 } declarations
                && declarations[0].GetSyntax(cancellationToken) is VariableDeclaratorSyntax { Initializer: null } fieldSyntax
                && !GeneratorTools.ContainsErrors(fieldSyntax))
            .ToList();

    private static IReadOnlyCollection<IPropertySymbol> GetPropertiesCore(INamedTypeSymbol type, CancellationToken cancellationToken)
        => type.GetMembers()
            .Where(member =>
                member.Kind == SymbolKind.Property
                && !member.IsStatic
                && !member.IsAbstract
                && member.IsDefinition
                && !member.IsImplicitlyDeclared)
            .Cast<IPropertySymbol>()
            .Where(property =>
                !property.IsIndexer
                && (property.IsReadOnly
                    || GeneratorTools.IsNonNullableReferenceType(property.Type))
                && property.DeclaringSyntaxReferences
                    .Select(declaration => declaration.GetSyntax(cancellationToken))
                    .OfType<PropertyDeclarationSyntax>()
                    .Any(propertySyntax => propertySyntax.Initializer is null
                        && !GeneratorTools.ContainsErrors(propertySyntax)
                        && propertySyntax.AccessorList?.Accessors.Any(accessor => accessor is { Body: null, ExpressionBody: null }
                            && accessor.IsKind(SyntaxKind.GetAccessorDeclaration)) == true))
            .ToList();
}
