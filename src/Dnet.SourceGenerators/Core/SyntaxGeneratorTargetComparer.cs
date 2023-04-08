using System.Collections.Generic;

namespace Dnet.SourceGenerators.Core;

/// <summary>
///    An equality comparer for an <see cref="ISyntaxGeneratorTarget"/>.
/// </summary>
public sealed class SyntaxGeneratorTargetComparer : IEqualityComparer<ISyntaxGeneratorTarget>
{
    private SyntaxGeneratorTargetComparer()
    {
    }

    /// <summary>
    ///   Gets the singleton instance of the <see cref="SyntaxGeneratorTargetComparer"/>.
    /// </summary>
    public static SyntaxGeneratorTargetComparer Instance { get; } = new();

    /// <inheritdoc/>
    public bool Equals(ISyntaxGeneratorTarget x, ISyntaxGeneratorTarget y)
        => x.SyntaxNode.Equals(y.SyntaxNode);

    /// <inheritdoc/>
    public int GetHashCode(ISyntaxGeneratorTarget obj) => obj.SyntaxNode.GetHashCode();
}
