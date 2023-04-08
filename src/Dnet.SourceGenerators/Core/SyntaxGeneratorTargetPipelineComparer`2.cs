using System.Collections.Generic;

namespace Dnet.SourceGenerators.Core;

/// <summary>
///     An equality comparer for a generator pipeline value containing an <see cref="ISyntaxGeneratorTarget"/> and another type.
/// </summary>
///
/// <typeparam name="TTarget">The target pipeline value type.</typeparam>
/// <typeparam name="TOther">The other pipeline value type.</typeparam>
public sealed class SyntaxGeneratorTargetPipelineComparer<TTarget, TOther> : IEqualityComparer<(TTarget Target, TOther _)>
    where TTarget : class, ISyntaxGeneratorTarget
{
    private SyntaxGeneratorTargetPipelineComparer()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the <see cref="SyntaxGeneratorTargetPipelineComparer{TTarget, TOther}"/>.
    /// </summary>
    public static SyntaxGeneratorTargetPipelineComparer<TTarget, TOther> Instance { get; } = new();

    /// <inheritdoc/>
    public bool Equals(
        (TTarget Target, TOther _) x,
        (TTarget Target, TOther _) y)
        => SyntaxGeneratorTargetComparer.Instance.Equals(x.Target, y.Target);

    /// <inheritdoc/>
    public int GetHashCode((TTarget Target, TOther _) obj)
        => SyntaxGeneratorTargetComparer.Instance.GetHashCode(obj.Target);
}
