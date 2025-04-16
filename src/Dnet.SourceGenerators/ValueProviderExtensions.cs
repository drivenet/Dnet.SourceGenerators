namespace Microsoft.CodeAnalysis;

public static class ValueProviderExtensions
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> self)
        where T : class
        => self.Where(value => value is not null)!;
}
