using KoreForge.SwaggerControllers;

namespace Sample.Test.Sample.V1;

/// <summary>
/// Business logic decorator — add validation, transformation, orchestration here.
/// All methods pass through to the inner service by default.
/// Scaffolded once and never overwritten by the generator.
/// </summary>
internal sealed class SampleBusinessDecorator : ISampleService
{
    private readonly ISampleService _inner;

    public SampleBusinessDecorator(ISampleService inner)
    {
        _inner = inner;
    }

    public Task<Outcome<IReadOnlyList<Widget>>> ListWidgetsAsync(int? pageSize, CancellationToken cancellationToken = default)
    {
        // TODO: Add business rules, validation, or transformation logic
        return _inner.ListWidgetsAsync(pageSize, cancellationToken);
    }

    public Task<Outcome<Widget>> CreateWidgetAsync(WidgetRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Add business rules, validation, or transformation logic
        return _inner.CreateWidgetAsync(request, cancellationToken);
    }

    public Task<Outcome<Widget>> GetWidgetAsync(string id, CancellationToken cancellationToken = default)
    {
        // TODO: Add business rules, validation, or transformation logic
        return _inner.GetWidgetAsync(id, cancellationToken);
    }
}
