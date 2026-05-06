namespace KoreForge.SwaggerControllers;

public sealed class ExternalErrorDetail
{
    public required string DependencyName { get; init; }
    public int? HttpStatusCode { get; init; }
    public string? RawResponse { get; init; }
}
