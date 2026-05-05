namespace KoreForge.SwaggerControllers;

/// <summary>
/// Classification of an error independent of its <see cref="ErrorOrigin"/>.
/// </summary>
public enum ErrorKind
{
    /// <summary>Authorization failure (caller lacks required role/permission).</summary>
    Authorization = 0,

    /// <summary>Technical/infrastructure failure (network, database, serialization, etc.).</summary>
    Technical = 1,

    /// <summary>Business/logical failure (invalid state, business rule violation, etc.).</summary>
    Logical = 2,
}
