namespace KoreForge.SwaggerControllers;

/// <summary>
/// Marker payload type for void-returning outcomes. <see cref="Outcome{T}"/>
/// with <c>T = Unit</c> maps to HTTP 204 No Content via
/// <see cref="OutcomeExtensions.ToActionResult{T}"/>.
/// </summary>
public sealed class Unit
{
    /// <summary>Singleton value.</summary>
    public static readonly Unit Value = new();

    private Unit() { }
}
