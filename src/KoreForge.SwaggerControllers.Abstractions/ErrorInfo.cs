namespace KoreForge.SwaggerControllers;

public sealed class ErrorInfo
{
    public required ErrorOrigin Origin { get; init; }
    public required ErrorKind Kind { get; init; }
    public required string Code { get; init; }
    public required string Message { get; init; }
    public ExternalErrorDetail? ExternalDetail { get; init; }
}
