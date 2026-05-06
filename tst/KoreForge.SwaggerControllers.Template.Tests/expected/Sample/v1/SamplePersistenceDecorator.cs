using KoreForge.SwaggerControllers;

namespace Sample.Test.Sample.V1;

/// <summary>
/// Persistence decorator — add auditing or DB writes here.
/// All methods pass through to the inner service by default.
/// </summary>
internal sealed class SamplePersistenceDecorator : ISampleService
{
    private readonly ISampleService _inner;

    public SamplePersistenceDecorator(ISampleService inner)
    {
        _inner = inner;
    }

    public Task<Outcome<IReadOnlyList<Widget>>> ListWidgetsAsync(int? pageSize, CancellationToken cancellationToken = default)
    {
        // TODO: Add persistence logic before/after the call
        return _inner.ListWidgetsAsync(pageSize, cancellationToken);
    }

    public Task<Outcome<Widget>> CreateWidgetAsync(WidgetRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Add persistence logic before/after the call
        return _inner.CreateWidgetAsync(request, cancellationToken);
    }

    public Task<Outcome<Widget>> GetWidgetAsync(string id, CancellationToken cancellationToken = default)
    {
        // TODO: Add persistence logic before/after the call
        return _inner.GetWidgetAsync(id, cancellationToken);
    }
}
