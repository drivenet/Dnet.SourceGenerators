using System;
using System.Text;

using Microsoft.CodeAnalysis.Text;

namespace Dnet.SourceGenerators;

/// <summary>
///    A source code file.
/// </summary>
///
/// <param name="Name">The source file name.</param>
/// <param name="Text">The source text.</param>
///
/// <exception cref="ArgumentOutOfRangeException">The source file name is empty.</exception>
public sealed record SourceInfo(string Name, SourceText Text)
{
    /// <summary>
    ///     Represents a preferred default encoding for source texts.
    /// </summary>
    public static readonly Encoding DefaultEncoding = new UTF8Encoding(true, false);

    /// <summary>
    ///     Initializes an instance of <see cref="SourceInfo"/> with default settings.
    /// </summary>
    ///
    /// <param name="name">The source file name.</param>
    /// <param name="text">The source text.</param>
    public SourceInfo(string name, string text)
        : this(name, SourceText.From(text, DefaultEncoding))
    {
    }

    /// <summary>
    ///     The source file name.
    /// </summary>
    public string Name { get; } = (Name ?? throw new ArgumentNullException(nameof(Name))) is { Length: not 0 }
        ? Name
        : throw new ArgumentOutOfRangeException(nameof(Name), "The source file name is empty.");

    /// <summary>
    ///     The source text.
    /// </summary>
    public SourceText Text { get; } = Text ?? throw new ArgumentNullException(nameof(Text));
}
