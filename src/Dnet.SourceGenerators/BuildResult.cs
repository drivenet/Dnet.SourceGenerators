using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dnet.SourceGenerators;

/// <summary>
///     A result of a build, usually by an implementation of <see cref="ISourceBuilder{TTarget}"/>.
/// </summary>
public sealed class BuildResult
{
    /// <summary>
    ///     An empty build result.
    /// </summary>
    public static readonly BuildResult Empty = new();

    private readonly string? _name;
    private readonly SourceText? _text;
    private readonly Diagnostic? _diagnostic;

    /// <summary>
    ///     Initializes an instance of a <see cref="BuildResult"/>, representing a successful named build result.
    /// </summary>
    ///
    /// <param name="name">The result name.</param>
    /// <param name="text">The source text.</param>
    public BuildResult(string name, SourceText text)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>
    ///     Initializes an instance of a <see cref="BuildResult"/>, representing a successful unnamed build result.
    /// </summary>
    ///
    /// <param name="text">The source text.</param>
    public BuildResult(SourceText text)
    {
        _text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>
    ///     Initializes an instance of a <see cref="BuildResult"/>, representing a failed build result.
    /// </summary>
    ///
    /// <param name="diagnostic">An diagnostic.</param>
    public BuildResult(Diagnostic? diagnostic)
    {
        _diagnostic = diagnostic ?? Diagnostic.Create(GeneratorDiagnostics.MissingResultOrDiagnostic, null);
    }

    private BuildResult()
    {
    }

    internal void Apply(Action<string?, SourceText> textHandler, Action<Diagnostic> diagnosticHandler)
    {
        if (textHandler is null)
        {
            throw new ArgumentNullException(nameof(textHandler));
        }

        if (diagnosticHandler is null)
        {
            throw new ArgumentNullException(nameof(diagnosticHandler));
        }

        if (_diagnostic is not null)
        {
            diagnosticHandler(_diagnostic);
        }

        if (_text is not null)
        {
            textHandler(_name, _text);
        }
    }
}
