namespace KoreForge.SwaggerControllers;

/// <summary>
/// Detail captured from an upstream HTTP failure. Always populated when
/// <see cref="ErrorInfo.Origin"/> is <see cref="ErrorOrigin.External"/>.
/// </summary>
/// <param name="StatusCode">HTTP status code returned by the upstream, when known.</param>
/// <param name="Body">Raw upstream response body, truncated as configured by the host.</param>
/// <param name="UpstreamCorrelationId">Correlation id reported by the upstream, when present.</param>
public sealed record ExternalErrorDetail(int? StatusCode, string? Body, string? UpstreamCorrelationId);
