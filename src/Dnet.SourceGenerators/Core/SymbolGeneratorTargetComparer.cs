using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators.Core;

/// <summary>
///     An equality comparer for an <see cref="ISymbolGeneratorTarget"/>.
/// </summary>
public sealed class SymbolGeneratorTargetComparer : IEqualityComparer<ISymbolGeneratorTarget>
{
    private SymbolGeneratorTargetComparer()
    {
    }

    /// <summary>
    ///    Gets the singleton instance of the <see cref="SymbolGeneratorTargetComparer"/>.
    /// </summary>
    public static SymbolGeneratorTargetComparer Instance { get; } = new();

    /// <inheritdoc/>
    public bool Equals(ISymbolGeneratorTarget x, ISymbolGeneratorTarget y)
        => x.Symbol.Equals(y.Symbol, SymbolEqualityComparer.IncludeNullability);

    /// <inheritdoc/>
    public int GetHashCode(ISymbolGeneratorTarget obj)
        => SymbolEqualityComparer.IncludeNullability.GetHashCode(obj.Symbol);
}
