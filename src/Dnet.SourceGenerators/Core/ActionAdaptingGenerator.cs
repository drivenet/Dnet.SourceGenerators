using System;

using Microsoft.CodeAnalysis;

namespace Dnet.SourceGenerators.Core;

/// <summary>
///     A generator that adapts an <see cref="Action{IncrementalGeneratorInitializationContext}"/>.
/// </summary>
public sealed class ActionAdaptingGenerator : IIncrementalGenerator
{
    private Action<IncrementalGeneratorInitializationContext>? _action;

    public ActionAdaptingGenerator(Action<IncrementalGeneratorInitializationContext> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

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
