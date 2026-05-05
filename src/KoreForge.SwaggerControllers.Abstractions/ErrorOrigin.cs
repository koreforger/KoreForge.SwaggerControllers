namespace KoreForge.SwaggerControllers;

/// <summary>
/// Origin of an error: did it originate inside the host process (<see cref="Internal"/>)
/// or in an upstream service the host called (<see cref="External"/>).
/// </summary>
public enum ErrorOrigin
{
    /// <summary>Error produced inside the host (validation, authorization, persistence, technical).</summary>
    Internal = 0,

    /// <summary>Error produced by an upstream/external service.</summary>
    External = 1,
}
