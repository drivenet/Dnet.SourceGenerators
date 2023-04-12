using System;

using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators.Core;

/// <summary>
///     A generator that adapts an <see cref="Action{IncrementalGeneratorInitializationContext}"/>.
/// </summary>
public sealed class ActionAdaptingGenerator : IIncrementalGenerator
{
    private Action<IncrementalGeneratorInitializationContext>? _action;

    /// <summary>
    ///    Initializes a new instance of the <see cref="ActionAdaptingGenerator"/> class.
    /// </summary>
    ///
    /// <param name="action">An action to adapt.</param>
    public ActionAdaptingGenerator(Action<IncrementalGeneratorInitializationContext> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    ///     Implements <see cref="IIncrementalGenerator.Initialize"/>.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (_action is not { } action)
        {
            throw new InvalidOperationException("Multiple initializations are unexpected.");
        }

        _action = null;
        action(context);
    }
}
