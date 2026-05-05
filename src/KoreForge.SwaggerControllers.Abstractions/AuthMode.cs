namespace KoreForge.SwaggerControllers;

/// <summary>
/// Authentication mode for outbound calls to an upstream/external API. Controls
/// which headers <see cref="AuthDelegatingHandler"/> adds.
/// </summary>
public enum AuthMode
{
    /// <summary>Send only a bearer JWT in the configured JWT header.</summary>
    JwtOnly = 0,

    /// <summary>Send only the client-id and client-secret headers.</summary>
    ClientCredentials = 1,

    /// <summary>Send both a bearer JWT and the client-id/client-secret headers.</summary>
    Both = 2,
}
