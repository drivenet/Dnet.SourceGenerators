using Microsoft.CodeAnalysis.CSharp;

namespace Dnet.SourceGenerators;

/// <summary>
///     A generator target, containing a syntax node.
/// </summary>
public interface ISyntaxGeneratorTarget
{
    /// <summary>
    ///     The syntax node.
    /// </summary>
    CSharpSyntaxNode SyntaxNode { get; }
}
