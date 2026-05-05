namespace KoreForge.SwaggerControllers;

/// <summary>
/// Structured error description carried by <see cref="Outcome{T}.Error"/>.
/// </summary>
/// <param name="Origin">Where the error originated.</param>
/// <param name="Kind">Classification of the error.</param>
/// <param name="Message">Human-readable message safe to log; never includes secrets.</param>
/// <param name="External">Upstream detail when <paramref name="Origin"/> is <see cref="ErrorOrigin.External"/>; otherwise null.</param>
public sealed record ErrorInfo(
    ErrorOrigin Origin,
    ErrorKind Kind,
    string Message,
    ExternalErrorDetail? External = null);
