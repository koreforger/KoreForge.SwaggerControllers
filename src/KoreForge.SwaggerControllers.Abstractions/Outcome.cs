namespace KoreForge.SwaggerControllers;

/// <summary>
/// Result envelope returned by every controller action. Exactly one of
/// <see cref="Data"/> or <see cref="Error"/> is set, determined by <see cref="Success"/>.
/// </summary>
/// <typeparam name="T">Successful payload type.</typeparam>
public sealed class Outcome<T>
{
    private Outcome(bool success, T? data, ErrorInfo? error, string correlationId)
    {
        Success = success;
        Data = data;
        Error = error;
        CorrelationId = correlationId;
    }

    /// <summary>True when the operation succeeded; <see cref="Data"/> is then non-null (unless T is nullable).</summary>
    public bool Success { get; }

    /// <summary>Successful payload. Null when <see cref="Success"/> is false.</summary>
    public T? Data { get; }

    /// <summary>Error description. Null when <see cref="Success"/> is true.</summary>
    public ErrorInfo? Error { get; }

    /// <summary>Correlation id flowed through the entire request pipeline (logging, audit, upstream calls).</summary>
    public string CorrelationId { get; }

    /// <summary>Construct a success outcome.</summary>
    public static Outcome<T> Ok(T data, string correlationId) =>
        new(success: true, data: data, error: null, correlationId: correlationId);

    /// <summary>Construct a failure outcome.</summary>
    public static Outcome<T> Fail(ErrorInfo error, string correlationId) =>
        new(success: false, data: default, error: error, correlationId: correlationId);
}
