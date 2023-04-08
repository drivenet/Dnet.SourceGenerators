using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators;

/// <summary>
///     A generator target, containing a symbol.
/// </summary>
public interface ISymbolGeneratorTarget
{
    /// <summary>
    ///     The symbol.
    /// </summary>
    ISymbol Symbol { get; }
}
