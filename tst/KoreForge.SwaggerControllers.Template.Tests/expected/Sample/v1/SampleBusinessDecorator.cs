using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KoreForge.SwaggerControllers;
using Sample.Test.Sample.V1.Dtos;

namespace Sample.Test.Sample.V1;

/// <summary>
/// Hand-written business-rule decorator. Pass-through by default; add validation, authorization, or cross-API orchestration here.
/// </summary>
internal sealed class SampleBusinessDecorator : ISampleService
{
    private readonly ISampleService _inner;

    public SampleBusinessDecorator(ISampleService inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }


    public Task<Outcome<IReadOnlyList<Widget>>> ListWidgetsAsync(int pageSize, string correlationId, CancellationToken cancellationToken = default)
        // TODO: business rules.
        => _inner.ListWidgetsAsync(pageSize, correlationId, cancellationToken);

    public Task<Outcome<Widget>> CreateWidgetAsync(WidgetRequest request, string correlationId, CancellationToken cancellationToken = default)
        // TODO: business rules.
        => _inner.CreateWidgetAsync(request, correlationId, cancellationToken);

    public Task<Outcome<Widget>> GetWidgetAsync(string id, string correlationId, CancellationToken cancellationToken = default)
        // TODO: business rules.
        => _inner.GetWidgetAsync(id, correlationId, cancellationToken);
}