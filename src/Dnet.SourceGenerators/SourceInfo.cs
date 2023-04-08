using System;

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
