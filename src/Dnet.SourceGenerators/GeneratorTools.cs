using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dnet.SourceGenerators;

/// <summary>
///     A collection of helper methods for source generators.
/// </summary>
public static class GeneratorTools
{
    /// <summary>
    ///     Tests if the attribute has a boolean named argument set to <c>true</c>.
    /// </summary>
    ///
    /// <param name="attribute">The attribute to test.</param>
    /// <param name="name">The argument name to test.</param>
    ///
    /// <returns><c>true</c>, if the attribute has a named argument set to <c>true</c>; elseway <c>false</c>.</returns>
    public static bool IsBoolSet(AttributeData attribute, string name)
    {
        if (attribute is null)
        {
            throw new ArgumentNullException(nameof(attribute));
        }

        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return attribute.NamedArguments.FirstOrDefault(pair => pair.Key == name).Value.Value is true;
    }

    /// <summary>
    ///     Checks if the syntax node contains any errors.
    /// </summary>
    ///
    /// <param name="node">The syntax node to check.</param>
    ///
    /// <returns><c>true</c>, if the syntax node contains any errors; otherwise <c>false</c>.</returns>
    public static bool ContainsErrors(CSharpSyntaxNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        return node
            .GetDiagnostics()
            .Any(d => d.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>
    ///    Tests if the type is a non-nullable reference type.
    /// </summary>
    ///
    /// <param name="type">The type to test.</param>
    ///
    /// <returns><c>true</c>, if the type is a non-nullable reference type; otherwise <c>false</c>.</returns>
    public static bool IsNonNullableReferenceType(ITypeSymbol type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return type.IsReferenceType
            && type.NullableAnnotation is NullableAnnotation.NotAnnotated or NullableAnnotation.None;
    }

    /// <summary>
    ///     Gets the top-level type accessibility.
    /// </summary>
    ///
    /// <param name="type">The type to get accessibility for.</param>
    ///
    /// <returns>The type accessibility or <c>null</c>, if the accessibility is incompatible with top-level types.</returns>
    public static string? GetTopLevelAccessibility(ITypeSymbol type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return type.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            _ => null,
        };
    }

    /// <summary>
    ///     Gets the top-level type accessibility.
    /// </summary>
    ///
    /// <param name="symbol">The type to get accessibility for.</param>
    ///
    /// <returns>The type accessibility or <c>null</c>, if the accessibility is incompatible with top-level types.</returns>
    public static string? GetMemberAccessibility(ISymbol symbol)
    {
        if (symbol is null)
        {
            throw new ArgumentNullException(nameof(symbol));
        }

        return symbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            _ => null,
        };
    }

    /// <summary>
    ///     Gets the name text from a name syntax node.
    /// </summary>
    ///
    /// <param name="node">The name syntax node.</param>
    ///
    /// <returns>The name text or <c>null</c>, if the node is unsupported.</returns>
    public static string? GetNameText(NameSyntax? node)
        => node switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null,
        };

    /// <summary>
    ///     Robustly executes an action and reports a diagnostic if an exception occurs.
    /// </summary>
    ///
    /// <param name="context">The target context.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="diagnosticFactory">A <see cref="Diagnostic"/> factory to use.</param>
    public static void RobustExecute(
        SourceProductionContext context,
        Action<SourceProductionContext> action,
        Func<Exception, Diagnostic> diagnosticFactory)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (diagnosticFactory is null)
        {
            throw new ArgumentNullException(nameof(diagnosticFactory));
        }

        try
        {
            action(context);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types -- required for robustness
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            ReportExceptionDiagnostic(context, exception, diagnosticFactory);
        }
    }

    /// <summary>
    ///     Robustly executes an action that produces a result and reports a diagnostic if an exception occurs.
    /// </summary>
    ///
    /// <typeparam name="TResult">The action result type.</typeparam>
    ///
    /// <param name="context">The target context.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="diagnosticFactory">A <see cref="Diagnostic"/> factory to use.</param>
    /// <param name="defaultResult">The result that is used when action fails.</param>
    ///
    /// <returns>The action result or <paramref name="defaultResult"/> if the action fails.</returns>
    public static TResult RobustExecute<TResult>(
        SourceProductionContext context,
        Func<SourceProductionContext, TResult> action,
        Func<Exception, Diagnostic> diagnosticFactory,
        TResult defaultResult)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (diagnosticFactory is null)
        {
            throw new ArgumentNullException(nameof(diagnosticFactory));
        }

        try
        {
            return action(context);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types -- required for robustness
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            ReportExceptionDiagnostic(context, exception, diagnosticFactory);
        }

        return defaultResult;
    }

    /// <summary>
    ///    Creates a diagnostic for an unhandled exception.
    /// </summary>
    /// <param name="exception">The unhandled exception.</param>
    /// <param name="location">The location where exception could occur.</param>
    /// <param name="target">The target for contextful exception.</param>
    ///
    /// <returns>The diagnostic.</returns>
    public static Diagnostic CreateExceptionDiagnostic(Exception exception, Location? location, object? target = null)
    {
        if (target is null)
        {
            return Diagnostic.Create(
                GeneratorDiagnostics.UnhandledException,
                location,
                exception?.GetType(),
                exception?.Message);
        }

        return Diagnostic.Create(
            GeneratorDiagnostics.ContextfulUnhandledException,
            location,
            target,
            exception?.GetType(),
            exception?.Message);
    }

    /// <summary>
    ///     Attempts to check type for extendability and reports a diagnostic if it is not.
    /// </summary>
    ///
    /// <typeparam name="TDeclaration">The declaration node type.</typeparam>
    ///
    /// <param name="target">The declaration target.</param>
    /// <param name="accessibility">The resulting type accessibility if the type is extendable; elseway <c>null</c>.</param>
    /// <param name="error">An extendability error diagnostic if the type is non-extendable; elseway <c>null</c>.</param>
    ///
    /// <returns><c>true</c>, if the type is extendable; elseway <c>false</c>.</returns>
    public static bool TryExtend<TDeclaration>(
        DeclaredSymbolTarget<TDeclaration, INamedTypeSymbol> target,
        [NotNullWhen(true)] out string? accessibility,
        [MaybeNullWhen(false)] out Diagnostic? error)
        where TDeclaration : TypeDeclarationSyntax
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var location = target.Declaration.GetLocation();
        var type = target.Type;
        if (!target.Declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            accessibility = null;
            error = Diagnostic.Create(GeneratorDiagnostics.MissingPartialKeyword, target.Declaration.Keyword.GetLocation(), type);
            return false;
        }

        if (target.Type.ContainingNamespace is not { IsGlobalNamespace: false })
        {
            accessibility = null;
            error = Diagnostic.Create(GeneratorDiagnostics.TopLevelTypesAreNotSupported, target.Declaration.Identifier.GetLocation(), target.Type);
            return false;
        }

        if (type.ContainingType is not null)
        {
            accessibility = null;
            error = Diagnostic.Create(GeneratorDiagnostics.ContainedTypesAreNotSupported, location, type);
            return false;
        }

        accessibility = GetTopLevelAccessibility(type);
        if (accessibility is null)
        {
            error = Diagnostic.Create(GeneratorDiagnostics.InvalidTopLevelTypeAccessibility, location, type);
            return false;
        }

        error = null;
        return true;
    }

    private static void ReportExceptionDiagnostic(SourceProductionContext context, Exception exception, Func<Exception, Diagnostic> diagnosticFactory)
    {
        try
        {
            var diagnostic = diagnosticFactory(exception);
            context.ReportDiagnostic(diagnostic);
            var exceptionInfo = "#error " + exception.ToString().Replace("\n", "\n//");
            context.AddSource("!" + diagnostic.Descriptor.Id + "-" + Guid.NewGuid(), exceptionInfo);
        }
#pragma warning disable CA1031 // Do not catch general exception types -- required for robustness
        catch
#pragma warning restore CA1031 // Do not catch general exception types
        {
        }
    }
}
